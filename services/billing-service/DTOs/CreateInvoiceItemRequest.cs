using System.ComponentModel.DataAnnotations;

namespace BillingService.DTOs;

public class CreateInvoiceItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }
}