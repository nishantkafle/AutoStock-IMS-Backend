using AutoStock.Application.DTOs.Vehicle;

namespace AutoStock.Application.Interfaces.IServices;

public interface IVehicleService
{
    // Customer views their own vehicles
    Task<ApiResponse<List<VehicleResponseDto>>> GetMyVehiclesAsync(string userId);

    // Customer adds a vehicle to their profile 
    Task<ApiResponse<VehicleResponseDto>> AddVehicleAsync(string userId, VehicleRequestDto dto);

    // Customer edits vehicle details
    Task<ApiResponse<VehicleResponseDto>> UpdateVehicleAsync(string userId, Guid vehicleId, VehicleRequestDto dto);

    // Customer removes a vehicle
    Task<ApiResponse<string>> DeleteVehicleAsync(string userId, Guid vehicleId);
}
