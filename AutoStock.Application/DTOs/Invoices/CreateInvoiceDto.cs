using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Invoices;

public class CreateInvoiceItemDto
{
    [Required]
    public Guid PartId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}

public class CreateInvoiceDto
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string CustomerPhone { get; set; } = string.Empty;

    public string CustomerAddress { get; set; } = string.Empty;

    [Required]
    public List<CreateInvoiceItemDto> Items { get; set; } = new();

    public decimal DiscountAmount { get; set; }

    public string PaymentMethod { get; set; } = "Cash";

    public decimal PaidAmount { get; set; }

    public string? CustomerId { get; set; }
}

public class SettleInvoiceDto
{
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
