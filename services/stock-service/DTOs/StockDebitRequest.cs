using System.ComponentModel.DataAnnotations;

namespace StockService.DTOs;

public class StockDebitRequest
{
    [Required]
    public Guid OperationId { get; set; }

    [Required]
    [MinLength(1)]
    public List<StockDebitItemRequest> Items { get; set; } = new();
}