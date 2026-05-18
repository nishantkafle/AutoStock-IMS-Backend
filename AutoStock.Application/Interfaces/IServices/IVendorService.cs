using AutoStock.Application.DTOs.Vendors;

namespace AutoStock.Application.Interfaces.IServices;

public interface IVendorService
{
    Task<ApiResponse<PagedResult<VendorDto>>> GetAllVendorsAsync(int page, int pageSize);
    Task<ApiResponse<VendorDto>> GetVendorByIdAsync(Guid id);
    Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto dto);
    Task<ApiResponse<bool>> UpdateVendorAsync(Guid id, UpdateVendorDto dto);
    Task<ApiResponse<bool>> DeleteVendorAsync(Guid id);
}
