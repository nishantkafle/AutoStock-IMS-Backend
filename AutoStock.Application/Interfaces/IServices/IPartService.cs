using AutoStock.Application.DTOs.Parts;

namespace AutoStock.Application.Interfaces.IServices;

public interface IPartService
{
    // Get all parts with pagination
    Task<ApiResponse<List<PartResponseDto>>> GetAllPartsAsync(int page, int pageSize);

    // Get one part by id
    Task<ApiResponse<PartResponseDto>> GetPartByIdAsync(Guid id);

    // Admin creates a new part
    Task<ApiResponse<PartResponseDto>> CreatePartAsync(PartRequestDto dto);

    // Admin edits an existing part
    Task<ApiResponse<PartResponseDto>> UpdatePartAsync(Guid id, PartRequestDto dto);

    // Admin deletes a part
    Task<ApiResponse<string>> DeletePartAsync(Guid id);

    // Get parts that are low on stock (for admin dashboard alerts)
    Task<ApiResponse<List<PartResponseDto>>> GetLowStockPartsAsync();
}
