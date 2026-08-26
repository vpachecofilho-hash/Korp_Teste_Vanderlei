using System.ComponentModel.DataAnnotations;

namespace BillingService.DTOs;

public class CreateInvoiceRequest
{
    [Required]
    [MinLength(1)]
    public List<CreateInvoiceItemRequest> Items { get; set; } = new();
}