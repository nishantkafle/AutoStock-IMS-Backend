using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string CustomerId { get; set; } = string.Empty;
    public User? Customer { get; set; }

    // 1 to 5 stars
    [Range(1, 5)]
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
