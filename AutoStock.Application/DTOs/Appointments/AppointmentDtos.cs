using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Appointments;

public class AppointmentRequestDto
{
    public Guid? VehicleId { get; set; }

    [Required]
    public string ServiceType { get; set; } = string.Empty;

    [Required]
    public DateTime AppointmentDate { get; set; }

    public string Notes { get; set; } = string.Empty;
}

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Staff or admin can update the status of an appointment
public class AppointmentStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}