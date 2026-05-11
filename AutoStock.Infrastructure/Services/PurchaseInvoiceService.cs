using AutoStock.Application;
using AutoStock.Application.DTOs.PurchaseInvoices;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class PurchaseInvoiceService(
    AppDbContext db,
    ILogger<PurchaseInvoiceService> logger
) : IPurchaseInvoiceService
{
    // Map a PurchaseInvoice entity to the response DTO
    private static PurchaseInvoiceResponseDto ToDto(PurchaseInvoice inv) => new()
    {
        Id = inv.Id,
        InvoiceNumber = inv.InvoiceNumber,
        VendorId = inv.VendorId,
        VendorName = inv.Vendor?.Name ?? "",
        CreatedByAdminName = inv.CreatedByAdmin?.FullName ?? "",
        TotalAmount = inv.TotalAmount,
        PurchaseDate = inv.PurchaseDate,
        Notes = inv.Notes,
        Items = inv.Items.Select(item => new PurchaseInvoiceItemResponseDto
        {
            Id = item.Id,
            PartId = item.PartId,
            PartName = item.Part?.Name ?? "",
            Quantity = item.Quantity,
            UnitCost = item.UnitCost,
            Subtotal = item.Subtotal
        }).ToList()
    };

    // Creates the invoice and updates stock in a single transaction 
    // If anything fails, both the invoice and the stock update are rolled back
    public async Task<ApiResponse<PurchaseInvoiceResponseDto>> CreateInvoiceAsync(
        string adminId, PurchaseInvoiceRequestDto dto)
    {
        // Validate vendor exists
        var vendor = await db.Vendors.FindAsync(dto.VendorId);
        if (vendor == null)
            return ApiResponse<PurchaseInvoiceResponseDto>.Fail("Vendor not found");

        // Validate all parts exist before starting transaction
        var partIds = dto.Items.Select(i => i.PartId).ToList();
        var parts = await db.Parts.Where(p => partIds.Contains(p.Id)).ToListAsync();

        if (parts.Count != partIds.Count)
            return ApiResponse<PurchaseInvoiceResponseDto>.Fail("One or more parts not found");

        // Use a transaction - invoice creation and stock update must both succeed or both fail
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Generate a unique invoice number
            var invoiceNumber = $"PI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            // Calculate total amount
            var totalAmount = dto.Items.Sum(i => i.Quantity * i.UnitCost);

            // Create the invoice
            var invoice = new PurchaseInvoice
            {
                VendorId = dto.VendorId,
                CreatedByAdminId = adminId,
                InvoiceNumber = invoiceNumber,
                TotalAmount = totalAmount,
                PurchaseDate = DateTime.SpecifyKind(dto.PurchaseDate, DateTimeKind.Utc),
                Notes = dto.Notes
            };

            // Create each line item and update the part stock
            foreach (var itemDto in dto.Items)
            {
                var part = parts.First(p => p.Id == itemDto.PartId);

                invoice.Items.Add(new PurchaseInvoiceItem
                {
                    PartId = itemDto.PartId,
                    Quantity = itemDto.Quantity,
                    UnitCost = itemDto.UnitCost
                });

                // Add purchased quantity to current stock - this is the stock update
                part.StockQty += itemDto.Quantity;
                part.UpdatedAt = DateTime.UtcNow;
            }

            db.PurchaseInvoices.Add(invoice);
            await db.SaveChangesAsync();

            // Both invoice and stock update succeeded - commit everything
            await transaction.CommitAsync();

            // Reload with navigation properties for the response
            var created = await db.PurchaseInvoices
                .Include(i => i.Vendor)
                .Include(i => i.CreatedByAdmin)
                .Include(i => i.Items).ThenInclude(item => item.Part)
                .FirstAsync(i => i.Id == invoice.Id);

            logger.LogInformation("Purchase invoice {InvoiceNumber} created by admin {AdminId}, total Rs.{Total}",
                invoiceNumber, adminId, totalAmount);

            return ApiResponse<PurchaseInvoiceResponseDto>.Ok(ToDto(created), "Purchase invoice created and stock updated");
        }
        catch (Exception ex)
        {
            // Something failed - roll back invoice AND stock changes
            await transaction.RollbackAsync();
            logger.LogError(ex, "Failed to create purchase invoice");
            return ApiResponse<PurchaseInvoiceResponseDto>.Fail("Failed to create invoice. No changes were saved.");
        }
    }

    // Get all invoices - newest first - with pagination 
    public async Task<ApiResponse<List<PurchaseInvoiceResponseDto>>> GetAllInvoicesAsync(int page, int pageSize)
    {
        var invoices = await db.PurchaseInvoices
            .Include(i => i.Vendor)
            .Include(i => i.CreatedByAdmin)
            .Include(i => i.Items).ThenInclude(item => item.Part)
            .OrderByDescending(i => i.PurchaseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<List<PurchaseInvoiceResponseDto>>.Ok(invoices.Select(ToDto).ToList());
    }

    public async Task<ApiResponse<PurchaseInvoiceResponseDto>> GetInvoiceByIdAsync(Guid id)
    {
        var invoice = await db.PurchaseInvoices
            .Include(i => i.Vendor)
            .Include(i => i.CreatedByAdmin)
            .Include(i => i.Items).ThenInclude(item => item.Part)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return ApiResponse<PurchaseInvoiceResponseDto>.Fail("Invoice not found");

        return ApiResponse<PurchaseInvoiceResponseDto>.Ok(ToDto(invoice));
    }
}
