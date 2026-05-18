using AutoStock.Application;
using AutoStock.Application.DTOs.Customer;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

// Feature 6 and 8 - Customer management service
public class CustomerService : ICustomerService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;   
    private readonly ILogger<CustomerService> _logger; 

    public CustomerService(
        UserManager<User> userManager,
        AppDbContext db,
        IEmailService emailService,             
        ILogger<CustomerService> logger)       
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
        _logger = logger;
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
        var vehicleExists = await _db.Vehicles
            .AnyAsync(v => v.VehicleNumber == dto.VehicleNumber);
        if (vehicleExists)
            return ApiResponse<CustomerResponseDto>.Fail(
                $"Vehicle number '{dto.VehicleNumber}' is already registered.");

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

        // Send welcome email with credentials + vehicle info
        try
        {
            string vehicleInfo = $"{dto.Make} {dto.Model} ({dto.Year}) - {dto.VehicleNumber}";

            await _emailService.SendCredentialsEmailAsync(
                toEmail: dto.Email,
                toName: dto.FullName,
                password: dto.Password,
                role: "Customer",
                extraInfo: vehicleInfo
            );
            _logger.LogInformation("Welcome email sent to customer {Email}", dto.Email);
        }
        catch (Exception ex)
        {
            // Don't fail registration if email fails - customer is still created
            _logger.LogWarning("Failed to send welcome email to customer {Email}: {Error}", dto.Email, ex.Message);
        }

        var response = MapToResponseDto(user, new List<Vehicle> { vehicle });
        return ApiResponse<CustomerResponseDto>.Ok(response, "Customer registered successfully");
    }

    // Feature 8: Staff views customer details and vehicle info
    public async Task<ApiResponse<CustomerResponseDto>> GetCustomerByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return ApiResponse<CustomerResponseDto>.Fail("Customer not found");

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

    // Staff searches customers by name, phone, ID, or vehicle number
    public async Task<ApiResponse<List<CustomerResponseDto>>> SearchCustomersAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return ApiResponse<List<CustomerResponseDto>>.Fail("Search keyword cannot be empty");

        keyword = keyword.ToLower();

        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        var filtered = customers.Where(c =>
            c.FullName.ToLower().Contains(keyword) ||
            (c.PhoneNumber != null && c.PhoneNumber.Contains(keyword)) ||
            c.Id.ToLower().Contains(keyword)
        ).ToList();

        var vehicleMatches = await _db.Vehicles
            .Where(v => v.VehicleNumber.ToLower().Contains(keyword))
            .Select(v => v.CustomerId)
            .ToListAsync();

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

    // Helper: map User + Vehicles to response DTO
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

    // Regular customers: registered more than 7 days ago or 2+ purchases
    public async Task<ApiResponse<List<CustomerReportDto>>> GetRegularCustomersAsync()
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var invoices = await _db.Invoices.ToListAsync();

        var regularPhones = invoices
            .Where(i => !string.IsNullOrWhiteSpace(i.CustomerPhone))
            .GroupBy(i => i.CustomerPhone)
            .Where(g => g.Count() >= 2)
            .Select(g => g.Key)
            .ToList();

        var regulars = customers
            .Where(c => c.CreatedAt <= sevenDaysAgo || (!string.IsNullOrWhiteSpace(c.PhoneNumber) && regularPhones.Contains(c.PhoneNumber)))
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

    // High spenders: customers with highest total invoice amounts
    public async Task<ApiResponse<List<CustomerReportDto>>> GetHighSpendersAsync()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var invoices = await _db.Invoices.ToListAsync();

        var spenders = customers
            .Select(c => new
            {
                Customer = c,
                TotalSpent = invoices
                    .Where(i => i.CustomerPhone == c.PhoneNumber)
                    .Sum(i => i.TotalAmount)
            })
            .Where(x => x.TotalSpent > 0)
            .OrderByDescending(x => x.TotalSpent)
            .Select(x => new CustomerReportDto
            {
                Id = x.Customer.Id,
                FullName = x.Customer.FullName,
                Email = x.Customer.Email ?? string.Empty,
                PhoneNumber = x.Customer.PhoneNumber ?? string.Empty,
                CreatedAt = x.Customer.CreatedAt,
                TotalSpent = x.TotalSpent
            }).ToList();

        return ApiResponse<List<CustomerReportDto>>.Ok(spenders);
    }

    // Pending credits: customers with unpaid balance
    public async Task<ApiResponse<List<CustomerReportDto>>> GetPendingCreditsAsync()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var invoices = await _db.Invoices.ToListAsync();

        var creditInvoices = invoices.Where(i => i.PaymentMethod == "Credit" && i.RemainingBalance > 0).ToList();

        var pendingMapById = creditInvoices
            .Where(i => !string.IsNullOrWhiteSpace(i.CustomerId))
            .GroupBy(i => i.CustomerId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.RemainingBalance));

        var pendingMapByPhone = creditInvoices
            .Where(i => string.IsNullOrWhiteSpace(i.CustomerId) && !string.IsNullOrWhiteSpace(i.CustomerPhone))
            .GroupBy(i => i.CustomerPhone)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.RemainingBalance));

        var pendingMapByName = creditInvoices
            .Where(i => string.IsNullOrWhiteSpace(i.CustomerId) && string.IsNullOrWhiteSpace(i.CustomerPhone) && !string.IsNullOrWhiteSpace(i.CustomerName))
            .GroupBy(i => i.CustomerName.ToLower().Trim())
            .ToDictionary(g => g.Key, g => g.Sum(i => i.RemainingBalance));

        var pending = customers
            .Where(c => pendingMapById.ContainsKey(c.Id) || 
                       (!string.IsNullOrWhiteSpace(c.PhoneNumber) && pendingMapByPhone.ContainsKey(c.PhoneNumber)) || 
                       (!string.IsNullOrWhiteSpace(c.FullName) && pendingMapByName.ContainsKey(c.FullName.ToLower().Trim())))
            .Select(c => new CustomerReportDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email ?? string.Empty,
                PhoneNumber = c.PhoneNumber ?? string.Empty,
                CreatedAt = c.CreatedAt,
                PendingCredit = pendingMapById.ContainsKey(c.Id) ? pendingMapById[c.Id] : 
                               (!string.IsNullOrWhiteSpace(c.PhoneNumber) && pendingMapByPhone.ContainsKey(c.PhoneNumber)) ? pendingMapByPhone[c.PhoneNumber] : 
                               pendingMapByName[c.FullName.ToLower().Trim()]
            }).ToList();

        return ApiResponse<List<CustomerReportDto>>.Ok(pending);
    }
}
