using AutoStock.Application;
using AutoStock.Application.DTOs.Parts;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class PartService(
    AppDbContext db,
    ILogger<PartService> logger
) : IPartService
{
    private static PartResponseDto ToDto(Part p) => new()
    {
        Id = p.Id,
        VendorId = p.VendorId,
        VendorName = p.Vendor?.Name ?? "",
        Name = p.Name,
        Description = p.Description,
        Category = p.Category,
        Price = p.Price,
        StockQty = p.StockQty,
        ReorderLevel = p.ReorderLevel,
        IsLowStock = p.StockQty < p.ReorderLevel,
        ImageUrl = p.ImageUrl,
        CreatedAt = p.CreatedAt
    };

    // IQueryable filters at DB level not in memory 
    public async Task<ApiResponse<List<PartResponseDto>>> GetAllPartsAsync(int page, int pageSize)
    {
        IQueryable<Part> query = db.Parts
            .Include(p => p.Vendor)
            .OrderBy(p => p.Name);

        var parts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        logger.LogInformation("Fetched {Count} parts for page {Page}", parts.Count, page);
        return ApiResponse<List<PartResponseDto>>.Ok(parts.Select(ToDto).ToList());
    }

    public async Task<ApiResponse<PartResponseDto>> GetPartByIdAsync(Guid id)
    {
        var part = await db.Parts
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (part == null)
            return ApiResponse<PartResponseDto>.Fail("Part not found");

        return ApiResponse<PartResponseDto>.Ok(ToDto(part));
    }

    public async Task<ApiResponse<PartResponseDto>> CreatePartAsync(PartRequestDto dto)
    {
        var vendorExists = await db.Vendors.AnyAsync(v => v.Id == dto.VendorId);
        if (!vendorExists)
            return ApiResponse<PartResponseDto>.Fail("Vendor not found");

        var part = new Part
        {
            VendorId = dto.VendorId,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Price = dto.Price,
            StockQty = dto.StockQty,
            ReorderLevel = dto.ReorderLevel
        };

        db.Parts.Add(part);
        await db.SaveChangesAsync();
        await db.Entry(part).Reference(p => p.Vendor).LoadAsync();

        logger.LogInformation("Part created: {Name}", part.Name);
        return ApiResponse<PartResponseDto>.Ok(ToDto(part), "Part created successfully");
    }

    public async Task<ApiResponse<PartResponseDto>> UpdatePartAsync(Guid id, PartRequestDto dto)
    {
        var part = await db.Parts.Include(p => p.Vendor).FirstOrDefaultAsync(p => p.Id == id);
        if (part == null)
            return ApiResponse<PartResponseDto>.Fail("Part not found");

        part.VendorId = dto.VendorId;
        part.Name = dto.Name;
        part.Description = dto.Description;
        part.Category = dto.Category;
        part.Price = dto.Price;
        part.StockQty = dto.StockQty;
        part.ReorderLevel = dto.ReorderLevel;
        part.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Part updated: {Name}", part.Name);
        return ApiResponse<PartResponseDto>.Ok(ToDto(part), "Part updated successfully");
    }

    public async Task<ApiResponse<string>> DeletePartAsync(Guid id)
    {
        var part = await db.Parts.FindAsync(id);
        if (part == null)
            return ApiResponse<string>.Fail("Part not found");

        db.Parts.Remove(part);
        await db.SaveChangesAsync();

        logger.LogInformation("Part deleted: {Id}", id);
        return ApiResponse<string>.Ok("Part deleted successfully");
    }

    public async Task<ApiResponse<List<PartResponseDto>>> GetLowStockPartsAsync()
    {
        var lowStock = await db.Parts
            .Include(p => p.Vendor)
            .Where(p => p.StockQty < p.ReorderLevel)
            .OrderBy(p => p.StockQty)
            .ToListAsync();

        return ApiResponse<List<PartResponseDto>>.Ok(lowStock.Select(ToDto).ToList());
    }
}
