using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Services;

public class CustomerHistoryService : ICustomerHistoryService
{
    private readonly AppDbContext _db;

    public CustomerHistoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CustomerHistoryResponseDto> GetHistoryAsync(string customerId)
    {
        var invoices = await _db.Invoices
            .Include(i => i.Items)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var purchases = invoices.Select(i => new PurchaseHistoryDto
        {
            Id = i.Id.ToString(),
            InvoiceNumber = "INV-" + i.Id.ToString().Substring(0, 8).ToUpper(),
            Date = i.CreatedAt,
            TotalAmount = i.TotalAmount,
            Status = i.RemainingBalance > 0 ? "Unpaid" : "Paid",
            Items = i.Items.Select(item => new PurchaseItemDto
            {
                PartName = item.PartName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
            }).ToList(),
        }).ToList();

        var paidRequests = await _db.PartRequests
            .Include(pr => pr.Part)
            .Where(pr => pr.CustomerId == customerId && pr.IsPaid)
            .OrderByDescending(pr => pr.RequestedAt)
            .ToListAsync();

        foreach (var pr in paidRequests)
        {
            purchases.Add(new PurchaseHistoryDto
            {
                Id = pr.Id.ToString(),
                InvoiceNumber = "PR-" + pr.Id.ToString().Substring(0, 8).ToUpper(),
                Date = pr.ResolvedAt ?? pr.RequestedAt,
                TotalAmount = (pr.Part?.Price ?? 0) * pr.Quantity,
                Status = "Paid",
                Items = new List<PurchaseItemDto>
                {
                    new PurchaseItemDto
                    {
                        PartName = pr.Part?.Name ?? pr.PartName,
                        Quantity = pr.Quantity,
                        UnitPrice = pr.Part?.Price ?? 0,
                        TotalPrice = (pr.Part?.Price ?? 0) * pr.Quantity,
                    }
                },
            });
        }

        purchases = purchases.OrderByDescending(p => p.Date).ToList();

        var appointments = await _db.Appointments
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        var services = appointments.Select(a => new ServiceHistoryDto
        {
            Id = a.Id.ToString(),
            Date = a.AppointmentDate,
            ServiceType = a.ServiceType,
            Status = a.Status,
            Notes = a.Notes,
        }).ToList();

        return new CustomerHistoryResponseDto
        {
            Purchases = purchases,
            Services = services,
        };
    }
}
