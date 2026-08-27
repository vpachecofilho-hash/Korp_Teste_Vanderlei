namespace BillingService.DTOs;

public class StockDebitItemRequest
{
    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }
}