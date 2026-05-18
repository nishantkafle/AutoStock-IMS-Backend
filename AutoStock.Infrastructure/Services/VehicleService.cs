using AutoStock.Application;
using AutoStock.Application.DTOs.Vehicle;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using AutoStock.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class VehicleService(
    AppDbContext db,
    ILogger<VehicleService> logger
) : IVehicleService
{
    private static VehicleResponseDto ToDto(Vehicle v) => new()
    {
        Id = v.Id,
        CustomerName = v.Customer?.FullName ?? "",
        Make = v.Make,
        Model = v.Model,
        Year = v.Year,
        VehicleNumber = v.VehicleNumber,
        Mileage = v.Mileage,
        LastServiceDate = v.LastServiceDate,
        CreatedAt = v.CreatedAt
    };

    public async Task<ApiResponse<List<VehicleResponseDto>>> GetMyVehiclesAsync(string userId)
    {
        var vehicles = await db.Vehicles
            .Include(v => v.Customer)
            .Where(v => v.CustomerId == userId)
            .OrderBy(v => v.Make)
            .ToListAsync();

        return ApiResponse<List<VehicleResponseDto>>.Ok(vehicles.Select(ToDto).ToList());
    }

    public async Task<ApiResponse<PagedResult<VehicleResponseDto>>> GetAllVehiclesAsync(int page, int pageSize)
    {
        var query = db.Vehicles
            .Include(v => v.Customer)
            .OrderBy(v => v.Customer!.FullName)
            .ThenBy(v => v.Make);

        var pagedVehicles = await query.ToPagedResultAsync(page, pageSize);
        var mappedItems = pagedVehicles.Items.Select(ToDto).ToList();
        var result = new PagedResult<VehicleResponseDto>(mappedItems, pagedVehicles.TotalCount, pagedVehicles.PageNumber, pagedVehicles.PageSize);

        return ApiResponse<PagedResult<VehicleResponseDto>>.Ok(result);
    }

    public async Task<ApiResponse<VehicleResponseDto>> AddVehicleAsync(string userId, VehicleRequestDto dto)
    {
        var exists = await db.Vehicles.AnyAsync(v => v.VehicleNumber == dto.VehicleNumber);
        if (exists)
            return ApiResponse<VehicleResponseDto>.Fail("Vehicle number already registered in the system");

        var vehicle = new Vehicle
        {
            CustomerId = userId,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            VehicleNumber = dto.VehicleNumber,
            Mileage = dto.Mileage
        };

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        logger.LogInformation("Vehicle added for customer {UserId}: {VehicleNumber}", userId, vehicle.VehicleNumber);
        return ApiResponse<VehicleResponseDto>.Ok(ToDto(vehicle), "Vehicle added successfully");
    }

    public async Task<ApiResponse<VehicleResponseDto>> UpdateVehicleAsync(string userId, Guid vehicleId, VehicleRequestDto dto)
    {
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.CustomerId == userId);

        if (vehicle == null)
            return ApiResponse<VehicleResponseDto>.Fail("Vehicle not found");

        vehicle.Make = dto.Make;
        vehicle.Model = dto.Model;
        vehicle.Year = dto.Year;
        vehicle.VehicleNumber = dto.VehicleNumber;
        vehicle.Mileage = dto.Mileage;

        await db.SaveChangesAsync();
        return ApiResponse<VehicleResponseDto>.Ok(ToDto(vehicle), "Vehicle updated");
    }

    public async Task<ApiResponse<string>> DeleteVehicleAsync(string userId, Guid vehicleId)
    {
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.CustomerId == userId);

        if (vehicle == null)
            return ApiResponse<string>.Fail("Vehicle not found");

        db.Vehicles.Remove(vehicle);
        await db.SaveChangesAsync();
        return ApiResponse<string>.Ok("Vehicle removed");
    }
}

