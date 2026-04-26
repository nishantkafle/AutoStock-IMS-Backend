using AutoStock.Application.DTOs.Auth;

namespace AutoStock.Application.Interfaces.IServices;

public interface IAuthService
{
    Task<ApiResponse<string>> RegisterCustomerAsync(RegisterDto dto);
    Task<ApiResponse<string>> RegisterStaffAsync(StaffRegistrationDto dto);
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
}