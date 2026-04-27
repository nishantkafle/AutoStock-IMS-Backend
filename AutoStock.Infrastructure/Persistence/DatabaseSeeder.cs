using AutoStock.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AutoStock.Infrastructure.Persistence;

// Creates default roles and one admin account on first startup 
public static class DatabaseSeeder
{
    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Staff", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Default admin 
        const string adminEmail = "admin@autostock.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new User
            {
                FullName = "System Admin",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Default staff
        const string staffEmail = "staff@test.com";
        if (await userManager.FindByEmailAsync(staffEmail) == null)
        {
            var staff = new User
            {
                FullName = "Test Staff",
                UserName = staffEmail,
                Email = staffEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(staff, "staff123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(staff, "Staff"); 
        }
    }
}