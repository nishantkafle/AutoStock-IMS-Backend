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

    // The user wants to support discount by 5%, 3% and customize amount
    // Let's pass the raw total discount amount from frontend, or discount percentage
    // For simplicity, let's just accept DiscountAmount
    public decimal DiscountAmount { get; set; }
}
