using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockService.Data;
using StockService.DTOs;
using StockService.Models;

namespace StockService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly StockDbContext _context;

    public ProductsController(StockDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(product => product.Code)
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Product>> GetById(Guid id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(
        CreateProductRequest request)
    {
        var code = request.Code.Trim();

        var productAlreadyExists = await _context.Products
            .AnyAsync(product => product.Code == code);

        if (productAlreadyExists)
        {
            return Conflict(new
            {
                message = "Um produto com este código já existe."
            });
        }

        var product = new Product
        {
            Code = code,
            Description = request.Description.Trim(),
            Stock = request.Stock
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product
        );
    }
}