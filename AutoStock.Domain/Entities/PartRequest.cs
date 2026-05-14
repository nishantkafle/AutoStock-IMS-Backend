using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class PartRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string CustomerId { get; set; } = string.Empty;
    public User? Customer { get; set; }

    public Guid? PartId { get; set; }
    public Part? Part { get; set; }

    [Required]
    public string PartName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public string Urgency { get; set; } = "Medium";

    public string Status { get; set; } = "Pending";

    public bool IsPaid { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }
}

