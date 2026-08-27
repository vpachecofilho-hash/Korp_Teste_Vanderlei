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
                message = "The same product cannot appear more than once in an invoice."
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
                        message = $"Product {item.ProductId} does not exist."
                    });
                }

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        StatusCodes.Status503ServiceUnavailable,
                        new
                        {
                            message = "Stock service is unavailable."
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
                    message = "Stock service is unavailable."
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
                message = "Invoice not found."
            });
        }

        if (invoice.Status != InvoiceStatus.Open)
        {
            return Conflict(new
            {
                message = "Only open invoices can be closed."
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
                    message = "Stock service is unavailable."
                }
            );
        }

        if (!stockResponse.IsSuccessStatusCode)
        {
            var stockError =
                await stockResponse.Content.ReadFromJsonAsync<ServiceErrorResponse>();

            return BadRequest(new
            {
                message = stockError?.Message ?? "Could not update stock."
            });
        }

        invoice.Status = InvoiceStatus.Closed;

        await _context.SaveChangesAsync();

        return Ok(invoice);
    }
}