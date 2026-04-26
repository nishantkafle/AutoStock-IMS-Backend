using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Vehicle;

public class VehicleRequestDto
{
    [Required]
    public string Make { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    public string VehicleNumber { get; set; } = string.Empty;

    public int Mileage { get; set; }
}

public class VehicleResponseDto
{
    public Guid Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime CreatedAt { get; set; }
}