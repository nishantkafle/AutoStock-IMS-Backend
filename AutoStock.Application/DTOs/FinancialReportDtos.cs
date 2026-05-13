namespace AutoStock.Application.DTOs;

// Daily report 

public class DailyReportDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }   // sum of SaleInvoices for that day
    public decimal TotalCost { get; set; }       // sum of PurchaseInvoices for that day
    public decimal GrossProfit => TotalRevenue - TotalCost;
    public int SalesCount { get; set; }
    public List<SaleSummaryDto> Sales { get; set; } = new();
}

public class SaleSummaryDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Time { get; set; }
}

// Monthly report 

public class MonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit => TotalRevenue - TotalCost;
    public int SalesCount { get; set; }

    // Revenue per day of the month 
    public List<DailyBreakdownDto> DailyBreakdown { get; set; } = new();
}

public class DailyBreakdownDto
{
    public int Day { get; set; }
    public decimal Revenue { get; set; }
    public int SalesCount { get; set; }
}

// Yearly report 

public class YearlyReportDto
{
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit => TotalRevenue - TotalCost;
    public int TotalSales { get; set; }

    // Revenue + cost per month 
    public List<MonthlyBreakdownDto> MonthlyBreakdown { get; set; } = new();
}

public class MonthlyBreakdownDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public int SalesCount { get; set; }
}
