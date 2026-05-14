using AutoStock.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoStock.Application;

namespace AutoStock.API.Controllers;

// DTOs just for profile - simple enough to keep here rather than a separate file
public class UpdateProfileDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(UserManager<User> userManager) : ControllerBase
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET api/profile - get own profile info
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = await userManager.FindByIdAsync(GetUserId());
        if (user == null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(ApiResponse<object>.Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? "",
            user.CreatedAt
        }));
    }

    // PUT api/profile - customer updates their display name and phone
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var user = await userManager.FindByIdAsync(GetUserId());
        if (user == null) return NotFound();

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        await userManager.UpdateAsync(user);

        return Ok(ApiResponse<string>.Ok("Profile updated successfully"));
    }

    // POST api/profile/change-password - any role can change own password
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await userManager.FindByIdAsync(GetUserId());
        if (user == null) return NotFound();

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.Fail(
                string.Join(", ", result.Errors.Select(e => e.Description))));

        return Ok(ApiResponse<string>.Ok("Password changed successfully"));
    }
}
