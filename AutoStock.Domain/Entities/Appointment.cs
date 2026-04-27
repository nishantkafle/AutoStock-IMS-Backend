using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Customer who booked
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    public User? Customer { get; set; }

    // Which vehicle this appointment is for
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    [Required]
    public string ServiceType { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    // Pending / Confirmed / Completed / Cancelled
    public string Status { get; set; } = "Pending";

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
