using BillingService.Data;
using BillingService.DTOs;
using BillingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace BillingService.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly BillingDbContext _context;

    private readonly IHttpClientFactory _httpClientFactory;

    public InvoicesController(
    BillingDbContext context,
    IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetAll()
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .OrderByDescending(invoice => invoice.Number)
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Invoice>> GetById(Guid id)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);

        if (invoice is null)
        {
            return NotFound();
        }

        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<Invoice>> Create(
        CreateInvoiceRequest request)
    {
        var duplicatedProduct = request.Items
            .GroupBy(item => item.ProductId)
            .Any(group => group.Count() > 1);

        if (duplicatedProduct)
        {
            return BadRequest(new
            {
                message = "O mesmo produto não pode aparecer mais de uma vez em uma fatura."
            });
        }
        
        var stockClient = _httpClientFactory.CreateClient("StockService");

        try
        {
            foreach (var item in request.Items)
            {
                var response = await stockClient.GetAsync(
                    $"/api/products/{item.ProductId}"
                );

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return BadRequest(new
                    {
                        message = $"Produto {item.ProductId} não existe."
                    });
                }

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        StatusCodes.Status503ServiceUnavailable,
                        new
                        {
                            message = "Serviço de estoque indisponível."
                        }
                    );
                }
            }
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = "Serviço de estoque indisponível."
                }
            );
        }

        var invoice = new Invoice
        {
            Status = InvoiceStatus.Open,
            Items = request.Items
                .Select(item => new InvoiceItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        _context.Invoices.Add(invoice);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            invoice
        );
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<Invoice>> Close(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);

        if (invoice is null)
        {
            return NotFound(new
            {
                message = "Fatura não encontrada."
            });
        }

        if (invoice.Status != InvoiceStatus.Open)
        {
            return Conflict(new
            {
                message = "Somente faturas abertas podem ser fechadas."
            });
        }

        var stockClient = _httpClientFactory.CreateClient("StockService");

        var debitRequest = new StockDebitRequest
        {
            OperationId = invoice.Id,

            Items = invoice.Items
                .Select(item => new StockDebitItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        HttpResponseMessage stockResponse;

        try
        {
            stockResponse = await stockClient.PostAsJsonAsync(
                "/api/stock/debit",
                debitRequest
            );
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = "Serviço de estoque indisponível."
                }
            );
        }

        if (!stockResponse.IsSuccessStatusCode)
        {
            var stockError =
                await stockResponse.Content.ReadFromJsonAsync<ServiceErrorResponse>();

            return BadRequest(new
            {
                message = stockError?.Message ?? "Não foi possível atualizar o estoque."
            });
        }

        invoice.Status = InvoiceStatus.Closed;

        await _context.SaveChangesAsync();

        return Ok(invoice);
    }
}