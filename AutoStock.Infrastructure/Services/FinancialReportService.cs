using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AutoStock.Infrastructure.Services;

public class FinancialReportService : IFinancialReportService
{
    private readonly AppDbContext _db;

    public FinancialReportService(AppDbContext db)
    {
        _db = db;
    }

    // DAILY 

    public async Task<DailyReportDto> GetDailyReportAsync(DateTime date)
    {
        // Strip time - compare only the date part
        var dayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var dayEnd = DateTime.SpecifyKind(dayStart.AddDays(1), DateTimeKind.Utc);

        // Revenue: Invoices created on that day
        var sales = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .Where(i => i.CreatedAt >= dayStart && i.CreatedAt < dayEnd)
            .ToListAsync();

        // Cost: PurchaseInvoices created on that day
        var purchases = await _db.PurchaseInvoices
            .Where(p => p.PurchaseDate >= dayStart && p.PurchaseDate < dayEnd)
            .ToListAsync();

        var totalRevenue = sales.Sum(s => s.TotalAmount);
        var totalCost = purchases.Sum(p => p.TotalAmount);

        return new DailyReportDto
        {
            Date = dayStart,
            TotalRevenue = totalRevenue,
            TotalCost = totalCost,
            SalesCount = sales.Count,
            Sales = sales.Select(s => new SaleSummaryDto
            {
                InvoiceNumber = s.Id.ToString().Substring(0, 8).ToUpper(),
                CustomerName = string.IsNullOrEmpty(s.CustomerName) ? (s.Customer?.FullName ?? "Unknown") : s.CustomerName,
                Amount = s.TotalAmount,
                Status = s.RemainingBalance <= 0 ? "Paid" : "Unpaid",
                Time = s.CreatedAt,
            }).OrderByDescending(s => s.Time).ToList(),
        };
    }

    // MONTHLY 

    public async Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month)
    {
        var monthStart = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
        var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1), DateTimeKind.Utc);

        var sales = await _db.Invoices
            .Where(i => i.CreatedAt >= monthStart && i.CreatedAt < monthEnd)
            .ToListAsync();

        var purchases = await _db.PurchaseInvoices
            .Where(p => p.PurchaseDate >= monthStart && p.PurchaseDate < monthEnd)
            .ToListAsync();

        // Build daily breakdown for the whole month
        int daysInMonth = DateTime.DaysInMonth(year, month);
        var dailyBreakdown = Enumerable.Range(1, daysInMonth).Select(day =>
        {
            var daySales = sales.Where(s => s.CreatedAt.Day == day).ToList();
            return new DailyBreakdownDto
            {
                Day = day,
                Revenue = daySales.Sum(s => s.TotalAmount),
                SalesCount = daySales.Count,
            };
        }).ToList();

        return new MonthlyReportDto
        {
            Year = year,
            Month = month,
            MonthName = monthStart.ToString("MMMM"),
            TotalRevenue = sales.Sum(s => s.TotalAmount),
            TotalCost = purchases.Sum(p => p.TotalAmount), 
            SalesCount = sales.Count,
            DailyBreakdown = dailyBreakdown,
        };
    }

    // YEARLY 

    public async Task<YearlyReportDto> GetYearlyReportAsync(int year)
    {
        var yearStart = DateTime.SpecifyKind(new DateTime(year, 1, 1), DateTimeKind.Utc);
        var yearEnd = DateTime.SpecifyKind(yearStart.AddYears(1), DateTimeKind.Utc);

        var sales = await _db.Invoices
            .Where(i => i.CreatedAt >= yearStart && i.CreatedAt < yearEnd)
            .ToListAsync();

        var purchases = await _db.PurchaseInvoices
            .Where(p => p.PurchaseDate >= yearStart && p.PurchaseDate < yearEnd)
            .ToListAsync();

        // Build monthly breakdown for all 12 months
        var monthlyBreakdown = Enumerable.Range(1, 12).Select(month =>
        {
            var monthSales = sales.Where(s => s.CreatedAt.Month == month).ToList();
            var monthPurchases = purchases.Where(p => p.PurchaseDate.Month == month).ToList();
            return new MonthlyBreakdownDto
            {
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month),
                Revenue = monthSales.Sum(s => s.TotalAmount),
                Cost = monthPurchases.Sum(p => p.TotalAmount), 
                SalesCount = monthSales.Count,
            };
        }).ToList();

        return new YearlyReportDto
        {
            Year = year,
            TotalRevenue = sales.Sum(s => s.TotalAmount),
            TotalCost = purchases.Sum(p => p.TotalAmount), 
            TotalSales = sales.Count,
            MonthlyBreakdown = monthlyBreakdown,
        };
    }
}
