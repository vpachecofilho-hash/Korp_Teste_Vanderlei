namespace BillingService.DTOs;

public class StockDebitRequest
{
    public Guid OperationId { get; set; }

    public List<StockDebitItemRequest> Items { get; set; } = new();
}