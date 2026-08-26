namespace BillingService.Models;
using System.Text.Json.Serialization;

public class InvoiceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    [JsonIgnore]
    public Invoice Invoice { get; set; } = null!;
}