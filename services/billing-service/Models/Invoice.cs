namespace BillingService.Models;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public long Number { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<InvoiceItem> Items { get; set; } = new();
}