using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Auth;

public class VerifyOtpDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Otp { get; set; } = string.Empty;
}