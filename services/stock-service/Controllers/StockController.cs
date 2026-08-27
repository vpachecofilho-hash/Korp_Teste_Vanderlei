using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockService.Data;
using StockService.DTOs;
using StockService.Models;

namespace StockService.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly StockDbContext _context;

    public StockController(StockDbContext context)
    {
        _context = context;
    }

    [HttpPost("debit")]
    public async Task<IActionResult> Debit(StockDebitRequest request)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable
            );

        try
        {
            var alreadyProcessed = await _context.StockOperations
                .AnyAsync(operation =>
                    operation.OperationId == request.OperationId);

            if (alreadyProcessed)
            {
                await transaction.RollbackAsync();

                return Ok(new
                {
                    message = "Operation already processed."
                });
            }

            var productIds = request.Items
                .Select(item => item.ProductId)
                .ToList();

            var products = await _context.Products
                .Where(product => productIds.Contains(product.Id))
                .ToListAsync();

            if (products.Count != productIds.Distinct().Count())
            {
                await transaction.RollbackAsync();

                return BadRequest(new
                {
                    message = "One or more products do not exist."
                });
            }

            foreach (var item in request.Items)
            {
                var product = products
                    .First(product => product.Id == item.ProductId);

                if (product.Stock < item.Quantity)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(new
                    {
                        message =
                            $"Insufficient stock for product {product.Code}."
                    });
                }
            }

            foreach (var item in request.Items)
            {
                var product = products
                    .First(product => product.Id == item.ProductId);

                product.Stock -= item.Quantity;
            }

            _context.StockOperations.Add(new StockOperation
            {
                OperationId = request.OperationId
            });

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Stock updated successfully."
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}