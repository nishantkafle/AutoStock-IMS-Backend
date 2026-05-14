using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Invoices;

public class UpdateInvoiceDto
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string CustomerPhone { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string CustomerAddress { get; set; } = string.Empty;

    [Required]
    public List<CreateInvoiceItemDto> Items { get; set; } = new();

    public decimal DiscountAmount { get; set; }

    public string PaymentMethod { get; set; } = "Cash";

    public decimal PaidAmount { get; set; }
}
