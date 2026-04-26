using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class Vendor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation - one vendor supplies many parts
    public ICollection<Part> Parts { get; set; } = [];
}
