using AutoStock.Application.DTOs.Appointments;

namespace AutoStock.Application.Interfaces.IServices;

public interface IAppointmentService
{
    // Customer books a new appointment
    Task<ApiResponse<AppointmentResponseDto>> BookAppointmentAsync(string customerId, AppointmentRequestDto dto);

    // Customer views their own appointments
    Task<ApiResponse<List<AppointmentResponseDto>>> GetMyAppointmentsAsync(string customerId);

    // Customer cancels their own appointment
    Task<ApiResponse<string>> CancelAppointmentAsync(string customerId, Guid appointmentId);

    // Admin or staff views all appointments
    Task<ApiResponse<PagedResult<AppointmentResponseDto>>> GetAllAppointmentsAsync(int page, int pageSize);

    // Admin or staff updates appointment status (Confirmed / Completed)
    Task<ApiResponse<AppointmentResponseDto>> UpdateStatusAsync(Guid appointmentId, AppointmentStatusDto dto);
}
