using AutoStock.Application.DTOs.Staff;

namespace AutoStock.Application.Interfaces.IServices;

public interface IStaffService
{
    // List all staff members
    Task<ApiResponse<List<StaffResponseDto>>> GetAllStaffAsync();

    // Get one staff member by id
    Task<ApiResponse<StaffResponseDto>> GetStaffByIdAsync(string id);

    // Admin creates a staff account (replaces the basic register-staff in AuthController)
    Task<ApiResponse<StaffResponseDto>> CreateStaffAsync(StaffCreateDto dto);

    // Admin edits name or active status
    Task<ApiResponse<StaffResponseDto>> UpdateStaffAsync(string id, StaffUpdateDto dto);

    // Admin deactivates staff (soft delete - does not remove from DB)
    Task<ApiResponse<string>> DeactivateStaffAsync(string id);
}
