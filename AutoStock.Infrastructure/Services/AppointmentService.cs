using AutoStock.Application;
using AutoStock.Application.DTOs.Appointments;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using AutoStock.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class AppointmentService(
    AppDbContext db,
    ILogger<AppointmentService> logger,
    IEmailService emailService
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
            AppointmentDate = DateTime.SpecifyKind(dto.AppointmentDate, DateTimeKind.Utc), 
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

        if (appointment.Customer?.Email != null)
        {
            await emailService.SendAppointmentEmailAsync(
                appointment.Customer.Email,
                appointment.Customer.FullName,
                "Appointment Booking Confirmation",
                $"Dear {appointment.Customer.FullName},\n\nYour appointment for {dto.ServiceType} has been successfully booked for {appointment.AppointmentDate:g}.\nWe will review and confirm it shortly.\n\nThank you,\nAutoStock IMS"
            );
        }

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
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.CustomerId == customerId);

        if (appointment == null)
            return ApiResponse<string>.Fail("Appointment not found");

        if (appointment.Status == "Completed")
            return ApiResponse<string>.Fail("Cannot cancel a completed appointment");

        appointment.Status = "Cancelled";
        await db.SaveChangesAsync();

        if (appointment.Customer?.Email != null)
        {
            await emailService.SendAppointmentEmailAsync(
                appointment.Customer.Email,
                appointment.Customer.FullName,
                "Appointment Cancelled",
                $"Dear {appointment.Customer.FullName},\n\nYour appointment for {appointment.ServiceType} on {appointment.AppointmentDate:g} has been successfully cancelled.\n\nThank you,\nAutoStock IMS"
            );
        }

        return ApiResponse<string>.Ok("Appointment cancelled");
    }

    // Admin and staff see all appointments
    public async Task<ApiResponse<PagedResult<AppointmentResponseDto>>> GetAllAppointmentsAsync(int page, int pageSize)
    {
        var query = db.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Vehicle)
            .OrderByDescending(a => a.AppointmentDate);

        var pagedAppointments = await query.ToPagedResultAsync(page, pageSize);
        var mappedItems = pagedAppointments.Items.Select(ToDto).ToList();
        var result = new PagedResult<AppointmentResponseDto>(mappedItems, pagedAppointments.TotalCount, pagedAppointments.PageNumber, pagedAppointments.PageSize);

        return ApiResponse<PagedResult<AppointmentResponseDto>>.Ok(result);
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

        if (appointment.Customer?.Email != null)
        {
            await emailService.SendAppointmentEmailAsync(
                appointment.Customer.Email,
                appointment.Customer.FullName,
                $"Appointment Status Update: {dto.Status}",
                $"Dear {appointment.Customer.FullName},\n\nThe status of your appointment for {appointment.ServiceType} on {appointment.AppointmentDate:g} has been updated to: {dto.Status}.\n\nThank you,\nAutoStock IMS"
            );
        }

        return ApiResponse<AppointmentResponseDto>.Ok(ToDto(appointment), "Status updated");
    }
}
