namespace AutoStock.Application.DTOs.Invoices;

public class InvoiceItemResponseDto
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class SettlementResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime SettlementDate { get; set; }
    public string? Notes { get; set; }
    public string? StaffId { get; set; }
}

public class InvoiceResponseDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? CustomerId { get; set; }
    public List<InvoiceItemResponseDto> Items { get; set; } = new();
    public List<SettlementResponseDto> Settlements { get; set; } = new();
}
