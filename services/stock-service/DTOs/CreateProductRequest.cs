using System.ComponentModel.DataAnnotations;

namespace StockService.DTOs;

public class CreateProductRequest
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Stock { get; set; }
}