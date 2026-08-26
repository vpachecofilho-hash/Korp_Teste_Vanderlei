namespace BillingService.DTOs;

public class ProductResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Stock { get; set; }
}