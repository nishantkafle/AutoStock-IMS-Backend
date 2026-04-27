using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Staff;

// Admin fills this to create a staff account
public class StaffCreateDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

// Admin sends this to update staff details
public class StaffUpdateDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

// Returned to frontend when listing or viewing staff
public class StaffResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
