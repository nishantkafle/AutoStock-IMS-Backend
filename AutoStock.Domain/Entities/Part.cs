using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace AutoStock.Domain.Entities;

public class Part
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // FK to Vendor who supplies this part
    public Guid VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQty { get; set; }

    // When stock drops below this, admin gets notified
    public int ReorderLevel { get; set; } = 10;

    // File path stored, not the file itself
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
