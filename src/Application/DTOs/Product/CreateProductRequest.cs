using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Product;

public class CreateProductRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    [Required]
    public Guid CategoryId { get; set; }
}
