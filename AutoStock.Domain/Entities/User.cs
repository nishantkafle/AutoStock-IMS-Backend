using Microsoft.AspNetCore.Identity;

namespace AutoStock.Domain.Entities;

// Extends IdentityUser which gives email, password hash, username 
public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}