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
        // Purchases: load sale invoices for this customer, newest first 
        var invoices = await _db.SaleInvoices
            .Include(i => i.Items)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var purchases = invoices.Select(i => new PurchaseHistoryDto
        {
            Id = i.Id.ToString(),
            InvoiceNumber = i.InvoiceNumber,
            Date = i.CreatedAt,
            TotalAmount = i.TotalAmount,
            Status = i.Status,
            Items = i.Items.Select(item => new PurchaseItemDto
            {
                PartName = item.PartName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
            }).ToList(),
        }).ToList();

        // Appointment entity uses CustomerId (string FK to ApplicationUser)
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
