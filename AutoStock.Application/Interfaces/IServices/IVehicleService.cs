using AutoStock.Application.DTOs.Vehicle;

namespace AutoStock.Application.Interfaces.IServices;

public interface IVehicleService
{
    Task<ApiResponse<List<VehicleResponseDto>>> GetMyVehiclesAsync(string userId);

    Task<ApiResponse<PagedResult<VehicleResponseDto>>> GetAllVehiclesAsync(int page, int pageSize);

    Task<ApiResponse<VehicleResponseDto>> AddVehicleAsync(string userId, VehicleRequestDto dto);

    Task<ApiResponse<VehicleResponseDto>> UpdateVehicleAsync(string userId, Guid vehicleId, VehicleRequestDto dto);

    Task<ApiResponse<string>> DeleteVehicleAsync(string userId, Guid vehicleId);
}
