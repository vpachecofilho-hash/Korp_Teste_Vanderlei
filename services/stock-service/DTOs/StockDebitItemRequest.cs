using System.ComponentModel.DataAnnotations;

namespace StockService.DTOs;

public class StockDebitItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }
}