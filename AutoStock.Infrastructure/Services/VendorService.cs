using AutoStock.Application.DTOs.Vendors;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly AppDbContext _context;

    public VendorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VendorDto>> GetAllVendorsAsync()
    {
        var vendors = await _context.Vendors.ToListAsync();
        return vendors.Select(MapToDto);
    }

    public async Task<VendorDto?> GetVendorByIdAsync(Guid id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        return vendor == null ? null : MapToDto(vendor);
    }

    public async Task<VendorDto> CreateVendorAsync(CreateVendorDto dto)
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

        return MapToDto(vendor);
    }

    public async Task<bool> UpdateVendorAsync(Guid id, UpdateVendorDto dto)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) return false;

        vendor.Name = dto.Name;
        vendor.ContactPerson = dto.ContactPerson ?? string.Empty;
        vendor.Phone = dto.Phone ?? string.Empty;
        vendor.Email = dto.Email ?? string.Empty;
        vendor.Address = dto.Address ?? string.Empty;
        vendor.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVendorAsync(Guid id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) return false;

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();
        return true;
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
