using AutoStock.Application.DTOs;

namespace AutoStock.Application.Interfaces;

public interface IFinancialReportService
{
    Task<DailyReportDto> GetDailyReportAsync(DateTime date);
    Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month);
    Task<YearlyReportDto> GetYearlyReportAsync(int year);
}
