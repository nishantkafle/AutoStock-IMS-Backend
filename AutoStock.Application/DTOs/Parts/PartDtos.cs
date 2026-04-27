using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Parts;

// Used when admin creates or edits a part
public class PartRequestDto
{
    [Required]
    public Guid VendorId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQty { get; set; }

    // Default reorder alert threshold is 10 
    public int ReorderLevel { get; set; } = 10;
}

// Returned to frontend when reading parts
public class PartResponseDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
