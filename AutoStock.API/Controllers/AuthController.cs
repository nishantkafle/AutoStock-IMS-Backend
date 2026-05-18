using AutoStock.Application.DTOs.Auth;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // Customer self-register - no auth required
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterCustomerAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var result = await authService.VerifyOtpAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Login works for all roles, returns JWT token
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    // Only admin can create staff accounts
    [HttpPost("register-staff")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto)
    {
        var result = await authService.RegisterStaffAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}