using AutoStock.Application.DTOs.Invoices;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Services;

public class InvoiceService(AppDbContext context) : IInvoiceService
{
    public async Task<InvoiceResponseDto> CreateInvoiceAsync(string staffId, CreateInvoiceDto dto)
    {
        var invoice = new Invoice
        {
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerAddress = dto.CustomerAddress,
            StaffId = staffId,
            DiscountAmount = dto.DiscountAmount,
            PaymentMethod = dto.PaymentMethod
        };

        decimal subTotal = 0;

        foreach (var itemDto in dto.Items)
        {
            var part = await context.Parts.FindAsync(itemDto.PartId);
            if (part == null) continue;

            // Optional: reduce stock quantity
            if (part.StockQty >= itemDto.Quantity)
            {
                part.StockQty -= itemDto.Quantity;
            }
            else
            {
                throw new Exception($"Not enough stock for part {part.Name}");
            }

            var totalPrice = part.Price * itemDto.Quantity;
            subTotal += totalPrice;

            invoice.Items.Add(new InvoiceItem
            {
                PartId = part.Id,
                PartName = part.Name,
                Quantity = itemDto.Quantity,
                UnitPrice = part.Price,
                TotalPrice = totalPrice
            });
        }

        invoice.SubTotal = subTotal;
        // ensure discount doesn't exceed subtotal
        var actualDiscount = dto.DiscountAmount > subTotal ? subTotal : dto.DiscountAmount;
        invoice.DiscountAmount = actualDiscount;
        invoice.TotalAmount = subTotal - actualDiscount;
        
        // Handle Partial Payment
        invoice.PaidAmount = dto.PaidAmount;
        invoice.RemainingBalance = invoice.TotalAmount - dto.PaidAmount;
        invoice.CustomerId = dto.CustomerId;

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        return MapToDto(invoice);
    }

    public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync()
    {
        var invoices = await context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Settlements)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invoices.Select(MapToDto);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid id)
    {
        var invoice = await context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Settlements)
            .FirstOrDefaultAsync(i => i.Id == id);

        return invoice == null ? null : MapToDto(invoice);
    }

    public async Task<InvoiceResponseDto> SettleInvoiceAsync(Guid id, string staffId, SettleInvoiceDto dto)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) throw new Exception("Invoice not found");

        var settlement = new CreditSettlement
        {
            Id = Guid.NewGuid(),
            InvoiceId = id,
            Amount = dto.Amount,
            Notes = dto.Notes,
            StaffId = staffId,
            SettlementDate = DateTime.UtcNow
        };

        // Explicitly add the settlement
        context.CreditSettlements.Add(settlement);

        // Update invoice totals
        invoice.PaidAmount += dto.Amount;
        invoice.RemainingBalance = invoice.TotalAmount - invoice.PaidAmount;
        
        // Ensure EF knows the invoice is modified
        context.Entry(invoice).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // If it fails, reload and try one more time
            context.Entry(invoice).Reload();
            invoice.PaidAmount += dto.Amount;
            invoice.RemainingBalance = invoice.TotalAmount - invoice.PaidAmount;
            await context.SaveChangesAsync();
        }

        // Return the full updated invoice
        return await GetInvoiceByIdAsync(id) ?? throw new Exception("Error reloading invoice");
    }

    private static InvoiceResponseDto MapToDto(Invoice invoice)
    {
        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            CustomerName = invoice.CustomerName,
            CustomerPhone = invoice.CustomerPhone,
            CustomerAddress = invoice.CustomerAddress,
            StaffId = invoice.StaffId,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            CreatedAt = invoice.CreatedAt,
            PaymentMethod = invoice.PaymentMethod,
            PaidAmount = invoice.PaidAmount,
            RemainingBalance = invoice.RemainingBalance,
            CustomerId = invoice.CustomerId,
            Items = (invoice.Items ?? new List<InvoiceItem>()).Select(i => new InvoiceItemResponseDto
            {
                Id = i.Id,
                PartId = i.PartId,
                PartName = i.PartName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList(),
            Settlements = (invoice.Settlements ?? new List<CreditSettlement>()).Select(s => new SettlementResponseDto
            {
                Id = s.Id,
                Amount = s.Amount,
                SettlementDate = s.SettlementDate,
                Notes = s.Notes,
                StaffId = s.StaffId
            }).OrderBy(s => s.SettlementDate).ToList()
        };
    }
}
