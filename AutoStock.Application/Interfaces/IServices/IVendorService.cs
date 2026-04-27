using AutoStock.Application.DTOs.Vendors;

namespace AutoStock.Application.Interfaces.IServices;

public interface IVendorService
{
    Task<IEnumerable<VendorDto>> GetAllVendorsAsync();
    Task<VendorDto?> GetVendorByIdAsync(Guid id);
    Task<VendorDto> CreateVendorAsync(CreateVendorDto dto);
    Task<bool> UpdateVendorAsync(Guid id, UpdateVendorDto dto);
    Task<bool> DeleteVendorAsync(Guid id);
}
