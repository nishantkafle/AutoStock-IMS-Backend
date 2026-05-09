using AutoStock.Application;
using AutoStock.Application.DTOs.Customer;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Services;

// Feature 6 and 8 - Customer management service
public class CustomerService : ICustomerService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;

    public CustomerService(UserManager<User> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    // Feature 6: Staff registers new customer with vehicle details
    public async Task<ApiResponse<CustomerResponseDto>> RegisterCustomerAsync(CustomerRegisterDto dto)
    {
        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return ApiResponse<CustomerResponseDto>.Fail("Email already registered");

        // Create the user
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ApiResponse<CustomerResponseDto>.Fail(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        // Assign Customer role
        await _userManager.AddToRoleAsync(user, "Customer");

        // Add vehicle
        var vehicle = new Vehicle
        {
            CustomerId = user.Id,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
           
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        var response = MapToResponseDto(user, new List<Vehicle> { vehicle });
        return ApiResponse<CustomerResponseDto>.Ok(response, "Customer registered successfully");
    }

    // Feature 8: Staff views customer details and vehicle info
    public async Task<ApiResponse<CustomerResponseDto>> GetCustomerByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return ApiResponse<CustomerResponseDto>.Fail("Customer not found");

        // Check if user is a customer
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Customer"))
            return ApiResponse<CustomerResponseDto>.Fail("User is not a customer");

        var vehicles = await _db.Vehicles
            .Where(v => v.CustomerId == user.Id)
            .ToListAsync();

        var response = MapToResponseDto(user, vehicles);
        return ApiResponse<CustomerResponseDto>.Ok(response);
    }

    // Feature 8: Staff views all customers
    public async Task<ApiResponse<List<CustomerResponseDto>>> GetAllCustomersAsync()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var result = new List<CustomerResponseDto>();

        foreach (var customer in customers)
        {
            var vehicles = await _db.Vehicles
                .Where(v => v.CustomerId == customer.Id)
                .ToListAsync();
            result.Add(MapToResponseDto(customer, vehicles));
        }

        return ApiResponse<List<CustomerResponseDto>>.Ok(result);
    }
    //  Staff searches customers by name, phone, ID, or vehicle number
    public async Task<ApiResponse<List<CustomerResponseDto>>> SearchCustomersAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return ApiResponse<List<CustomerResponseDto>>.Fail("Search keyword cannot be empty");

        keyword = keyword.ToLower();

        // Get all customers
        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        // Filter by name, phone, or ID
        var filtered = customers.Where(c =>
            c.FullName.ToLower().Contains(keyword) ||
            (c.PhoneNumber != null && c.PhoneNumber.Contains(keyword)) ||
            c.Id.ToLower().Contains(keyword)
        ).ToList();

        // Also search by vehicle number
        var vehicleMatches = await _db.Vehicles
            .Where(v => v.VehicleNumber.ToLower().Contains(keyword))
            .Select(v => v.CustomerId)
            .ToListAsync();

        // Add customers found by vehicle number
        foreach (var customerId in vehicleMatches)
        {
            var customer = customers.FirstOrDefault(c => c.Id == customerId);
            if (customer != null && !filtered.Any(f => f.Id == customer.Id))
                filtered.Add(customer);
        }

        var result = new List<CustomerResponseDto>();
        foreach (var customer in filtered)
        {
            var vehicles = await _db.Vehicles
                .Where(v => v.CustomerId == customer.Id)
                .ToListAsync();
            result.Add(MapToResponseDto(customer, vehicles));
        }

        return ApiResponse<List<CustomerResponseDto>>.Ok(result);
    }

    // Helper method to map User + Vehicles to response DTO
    private static CustomerResponseDto MapToResponseDto(User user, List<Vehicle> vehicles)
    {
        return new CustomerResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Vehicles = vehicles.Select(v => new CustomerVehicleDto
            {
                Id = v.Id,
                VehicleNumber = v.VehicleNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LastServiceDate = v.LastServiceDate
            }).ToList()
        };
    }
    //  Regular customers : registered more than 3 months ago
    public async Task<ApiResponse<List<CustomerReportDto>>> GetRegularCustomersAsync()
    {
        var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        var regulars = customers
            .Where(c => c.CreatedAt <= threeMonthsAgo)
            .Select(c => new CustomerReportDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email ?? string.Empty,
                PhoneNumber = c.PhoneNumber ?? string.Empty,
                CreatedAt = c.CreatedAt
            }).ToList();

        return ApiResponse<List<CustomerReportDto>>.Ok(regulars);
    }

    //  Pending credits : customers with unpaid balance
    public async Task<ApiResponse<List<CustomerReportDto>>> GetPendingCreditsAsync()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        var pending = customers
            .Where(c => c.HasPendingCredit)
            .Select(c => new CustomerReportDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email ?? string.Empty,
                PhoneNumber = c.PhoneNumber ?? string.Empty,
                CreatedAt = c.CreatedAt
            }).ToList();

        return ApiResponse<List<CustomerReportDto>>.Ok(pending);
    }
}