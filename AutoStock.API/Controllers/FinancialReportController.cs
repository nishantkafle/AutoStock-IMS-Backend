using AutoStock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/reports/financial")]
[Authorize(Roles = "Admin")]   // only Admin can access financial reports
public class FinancialReportController : ControllerBase
{
    private readonly IFinancialReportService _reportService;

    public FinancialReportController(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    // GET /api/reports/financial/daily?date=2026-05-11
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;
        var data = await _reportService.GetDailyReportAsync(targetDate);
        return Ok(new { success = true, data });
    }

    // GET /api/reports/financial/monthly?year=2026&month=5
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly(
        [FromQuery] int? year,
        [FromQuery] int? month)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        if (targetMonth < 1 || targetMonth > 12)
            return BadRequest(new { success = false, message = "Month must be 1-12." });

        var data = await _reportService.GetMonthlyReportAsync(targetYear, targetMonth);
        return Ok(new { success = true, data });
    }

    // GET /api/reports/financial/yearly?year=2026
    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearly([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var data = await _reportService.GetYearlyReportAsync(targetYear);
        return Ok(new { success = true, data });
    }
}
