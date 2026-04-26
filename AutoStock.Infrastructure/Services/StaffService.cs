using AutoStock.Application;
using AutoStock.Application.DTOs.Staff;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class StaffService(
    UserManager<User> userManager,
    ILogger<StaffService> logger
) : IStaffService
{
    private static StaffResponseDto ToDto(User u, string role) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email!,
        Role = role,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };

    // Get all users with Staff role
    public async Task<ApiResponse<List<StaffResponseDto>>> GetAllStaffAsync()
    {
        var staffUsers = await userManager.GetUsersInRoleAsync("Staff");
        var result = staffUsers.Select(u => ToDto(u, "Staff")).ToList();
        return ApiResponse<List<StaffResponseDto>>.Ok(result);
    }

    public async Task<ApiResponse<StaffResponseDto>> GetStaffByIdAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return ApiResponse<StaffResponseDto>.Fail("Staff member not found");

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Staff"))
            return ApiResponse<StaffResponseDto>.Fail("User is not a staff member");

        return ApiResponse<StaffResponseDto>.Ok(ToDto(user, "Staff"));
    }

    // Admin creates staff - assigns Staff role automatically
    public async Task<ApiResponse<StaffResponseDto>> CreateStaffAsync(StaffCreateDto dto)
    {
        if (await userManager.FindByEmailAsync(dto.Email) != null)
            return ApiResponse<StaffResponseDto>.Fail("Email already registered");

        var user = new User
        {
            FullName = dto.FullName,
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ApiResponse<StaffResponseDto>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Staff");
        logger.LogInformation("Admin created staff account for {Email}", dto.Email);
        return ApiResponse<StaffResponseDto>.Ok(ToDto(user, "Staff"), "Staff account created");
    }

    public async Task<ApiResponse<StaffResponseDto>> UpdateStaffAsync(string id, StaffUpdateDto dto)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return ApiResponse<StaffResponseDto>.Fail("Staff member not found");

        user.FullName = dto.FullName;
        user.IsActive = dto.IsActive;

        await userManager.UpdateAsync(user);
        logger.LogInformation("Staff updated: {Id}", id);
        return ApiResponse<StaffResponseDto>.Ok(ToDto(user, "Staff"), "Staff updated");
    }

    // Soft delete - just marks as inactive, keeps all history
    public async Task<ApiResponse<string>> DeactivateStaffAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return ApiResponse<string>.Fail("Staff member not found");

        user.IsActive = false;
        await userManager.UpdateAsync(user);
        logger.LogInformation("Staff deactivated: {Id}", id);
        return ApiResponse<string>.Ok("Staff member deactivated");
    }
}
