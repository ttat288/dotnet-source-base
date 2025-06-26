using Domain.Common;

namespace Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public Guid UserId { get; set; }
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public User User { get; set; } = null!;
}
