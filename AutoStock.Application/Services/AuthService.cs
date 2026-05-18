using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoStock.Application.DTOs.Auth;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AutoStock.Application.Services;

public class AuthService(
    UserManager<User> userManager,
    IConfiguration config,
    IEmailService emailService,
    ILogger<AuthService> logger
) : IAuthService
{
    // Customer registers themselves, automatically assigned Customer role
    public async Task<ApiResponse<string>> RegisterCustomerAsync(RegisterDto dto)
    {
        if (await userManager.FindByEmailAsync(dto.Email) != null)
        {
            logger.LogWarning("{Email} tried to register but already exists", dto.Email);
            return ApiResponse<string>.Fail("This email is already registered");
        }

        var user = new User
        {
            FullName = dto.FullName,
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ApiResponse<string>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Customer");

        try
        {
            var otp = new Random().Next(100000, 999999).ToString();
            OtpStore.Save(dto.Email, otp);
            await emailService.SendOtpAsync(dto.Email, otp);
            logger.LogInformation("Sent verification OTP to {Email}", dto.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification OTP to {Email}", dto.Email);
        }

        logger.LogInformation("{Email} registered as Customer", dto.Email);
        return ApiResponse<string>.Ok("Registration successful. Please verify your email with the code sent.");
    }

    // Admin creates staff accounts, staff cannot self-register
    public async Task<ApiResponse<string>> RegisterStaffAsync(StaffRegistrationDto dto)
    {
        if (await userManager.FindByEmailAsync(dto.Email) != null)
            return ApiResponse<string>.Fail("This email is already registered");

        var user = new User
        {
            FullName = dto.FullName,
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ApiResponse<string>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Staff");
        logger.LogInformation("Admin created Staff account for {Email}", dto.Email);
        return ApiResponse<string>.Ok("Staff account created successfully");
    }

    // Single login endpoint works for all three roles
    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user == null || !user.IsActive)
        {
            logger.LogWarning("Login failed for {Email}", dto.Email);
            return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");
        }

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Login failed for {Email} - email not confirmed", dto.Email);
            return ApiResponse<LoginResponseDto>.Fail("Please verify your email first.");
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";
        var token = GenerateToken(user, role);

        logger.LogInformation("{Email} logged in as {Role}", dto.Email, role);
        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email!,
            Role = role
        });
    }

    // Build JWT token containing user identity and role
    private string GenerateToken(User user, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<ApiResponse<string>> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            logger.LogWarning("Verification failed - email {Email} not found", dto.Email);
            return ApiResponse<string>.Fail("User not found");
        }

        if (user.EmailConfirmed)
        {
            return ApiResponse<string>.Ok("Email is already verified");
        }

        var isValid = OtpStore.Verify(dto.Email, dto.Otp);
        if (!isValid)
        {
            logger.LogWarning("Verification failed for {Email} - invalid or expired OTP {Otp}", dto.Email, dto.Otp);
            return ApiResponse<string>.Fail("Invalid or expired verification code");
        }

        user.EmailConfirmed = true;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to update user {Email} after OTP verification", dto.Email);
            return ApiResponse<string>.Fail("Failed to update user verification status");
        }

        OtpStore.Remove(dto.Email);
        logger.LogInformation("Email verified successfully for {Email}", dto.Email);
        return ApiResponse<string>.Ok("Email verified successfully");
    }
}