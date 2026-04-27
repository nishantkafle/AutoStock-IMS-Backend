using AutoStock.Application;
using AutoStock.Application.DTOs.Appointments;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class AppointmentService(
    AppDbContext db,
    ILogger<AppointmentService> logger
) : IAppointmentService
{
    private static AppointmentResponseDto ToDto(Appointment a) => new()
    {
        Id = a.Id,
        CustomerId = a.CustomerId,
        CustomerName = a.Customer?.FullName ?? "",
        VehicleId = a.VehicleId,
        VehicleNumber = a.Vehicle?.VehicleNumber ?? "",
        ServiceType = a.ServiceType,
        AppointmentDate = a.AppointmentDate,
        Status = a.Status,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt
    };

    public async Task<ApiResponse<AppointmentResponseDto>> BookAppointmentAsync(
        string customerId, AppointmentRequestDto dto)
    {
        var appointment = new Appointment
        {
            CustomerId = customerId,
            VehicleId = dto.VehicleId,
            ServiceType = dto.ServiceType,
            AppointmentDate = dto.AppointmentDate,
            Notes = dto.Notes,
            Status = "Pending"
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();

        // Load navigation properties for the response
        await db.Entry(appointment).Reference(a => a.Customer).LoadAsync();
        if (appointment.VehicleId.HasValue)
            await db.Entry(appointment).Reference(a => a.Vehicle).LoadAsync();

        logger.LogInformation("Appointment booked by customer {Id} for {ServiceType}",
            customerId, dto.ServiceType);

        return ApiResponse<AppointmentResponseDto>.Ok(ToDto(appointment), "Appointment booked successfully");
    }

    // Customer sees only their own appointments
    public async Task<ApiResponse<List<AppointmentResponseDto>>> GetMyAppointmentsAsync(string customerId)
    {
        var list = await db.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Vehicle)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return ApiResponse<List<AppointmentResponseDto>>.Ok(list.Select(ToDto).ToList());
    }

    // Customer can only cancel their own pending appointment
    public async Task<ApiResponse<string>> CancelAppointmentAsync(string customerId, Guid appointmentId)
    {
        var appointment = await db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.CustomerId == customerId);

        if (appointment == null)
            return ApiResponse<string>.Fail("Appointment not found");

        if (appointment.Status == "Completed")
            return ApiResponse<string>.Fail("Cannot cancel a completed appointment");

        appointment.Status = "Cancelled";
        await db.SaveChangesAsync();
        return ApiResponse<string>.Ok("Appointment cancelled");
    }

    // Admin and staff see all appointments
    public async Task<ApiResponse<List<AppointmentResponseDto>>> GetAllAppointmentsAsync()
    {
        var list = await db.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Vehicle)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return ApiResponse<List<AppointmentResponseDto>>.Ok(list.Select(ToDto).ToList());
    }

    // Admin or staff confirms / completes an appointment
    public async Task<ApiResponse<AppointmentResponseDto>> UpdateStatusAsync(
        Guid appointmentId, AppointmentStatusDto dto)
    {
        var allowed = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
        if (!allowed.Contains(dto.Status))
            return ApiResponse<AppointmentResponseDto>.Fail("Invalid status value");

        var appointment = await db.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return ApiResponse<AppointmentResponseDto>.Fail("Appointment not found");

        appointment.Status = dto.Status;
        await db.SaveChangesAsync();

        logger.LogInformation("Appointment {Id} status changed to {Status}", appointmentId, dto.Status);
        return ApiResponse<AppointmentResponseDto>.Ok(ToDto(appointment), "Status updated");
    }
}
