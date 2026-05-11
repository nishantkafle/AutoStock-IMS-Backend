namespace AutoStock.Application.DTOs;

// Purchase history (sale invoices) 

public class PurchaseHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PurchaseItemDto> Items { get; set; } = new();
}

public class PurchaseItemDto
{
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

// Service history (appointments) 

public class ServiceHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

// Combined response returned by the controller 

public class CustomerHistoryResponseDto
{
    public List<PurchaseHistoryDto> Purchases { get; set; } = new();
    public List<ServiceHistoryDto> Services { get; set; } = new();
}
