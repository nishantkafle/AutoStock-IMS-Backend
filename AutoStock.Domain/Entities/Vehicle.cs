using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Links to the customer who owns this vehicle
    public string CustomerId { get; set; } = string.Empty;
    public User? Customer { get; set; }

    [Required]
    public string Make { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    [Required]
    public string VehicleNumber { get; set; } = string.Empty;

    public int Mileage { get; set; }

    public DateTime? LastServiceDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
