using AutoStock.Application;
using AutoStock.Application.DTOs.Vendors;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using AutoStock.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly AppDbContext _context;

    public VendorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<VendorDto>>> GetAllVendorsAsync(int page, int pageSize)
    {
        var query = _context.Vendors.OrderBy(v => v.Name);

        var pagedVendors = await query.ToPagedResultAsync(page, pageSize);
        var mappedItems = pagedVendors.Items.Select(MapToDto).ToList();
        var result = new PagedResult<VendorDto>(mappedItems, pagedVendors.TotalCount, pagedVendors.PageNumber, pagedVendors.PageSize);

        return ApiResponse<PagedResult<VendorDto>>.Ok(result);
    }

    public async Task<ApiResponse<VendorDto>> GetVendorByIdAsync(Guid id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return ApiResponse<VendorDto>.Fail("Vendor not found");
        
        return ApiResponse<VendorDto>.Ok(MapToDto(vendor));
    }

    public async Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto dto)
    {
        var vendor = new Vendor
        {
            Name = dto.Name,
            ContactPerson = dto.ContactPerson ?? string.Empty,
            Phone = dto.Phone ?? string.Empty,
            Email = dto.Email ?? string.Empty,
            Address = dto.Address ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        return ApiResponse<VendorDto>.Ok(MapToDto(vendor), "Vendor created successfully");
    }

    public async Task<ApiResponse<bool>> UpdateVendorAsync(Guid id, UpdateVendorDto dto)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) 
            return ApiResponse<bool>.Fail("Vendor not found");

        vendor.Name = dto.Name;
        vendor.ContactPerson = dto.ContactPerson ?? string.Empty;
        vendor.Phone = dto.Phone ?? string.Empty;
        vendor.Email = dto.Email ?? string.Empty;
        vendor.Address = dto.Address ?? string.Empty;
        vendor.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Vendor updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteVendorAsync(Guid id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) 
            return ApiResponse<bool>.Fail("Vendor not found");

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Vendor deleted successfully");
    }

    private static VendorDto MapToDto(Vendor vendor)
    {
        return new VendorDto(
            vendor.Id,
            vendor.Name,
            vendor.ContactPerson,
            vendor.Phone,
            vendor.Email,
            vendor.Address,
            vendor.IsActive,
            vendor.CreatedAt
        );
    }
}
