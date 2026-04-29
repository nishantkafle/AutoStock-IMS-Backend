using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class PartRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Customer who requested the part
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    public User? Customer { get; set; }

    [Required]
    public string PartName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Low / Medium / High
    public string Urgency { get; set; } = "Medium";

    // Pending / Fulfilled / Rejected
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }
}
