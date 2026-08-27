namespace StockService.Models;

public class StockOperation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OperationId { get; set; }

    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}