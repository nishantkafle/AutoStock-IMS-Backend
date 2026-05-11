using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.PurchaseInvoices;

// One line item when admin creates the invoice
public class PurchaseInvoiceItemRequestDto
{
    [Required]
    public Guid PartId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than 0")]
    public decimal UnitCost { get; set; }
}

// Full invoice creation request from admin
public class PurchaseInvoiceRequestDto
{
    [Required]
    public Guid VendorId { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Invoice must have at least one item")]
    public List<PurchaseInvoiceItemRequestDto> Items { get; set; } = [];
}

// Single item shown in the invoice response
public class PurchaseInvoiceItemResponseDto
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Subtotal { get; set; }
}

// Full invoice response returned to frontend
public class PurchaseInvoiceResponseDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string CreatedByAdminName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseInvoiceItemResponseDto> Items { get; set; } = [];
}
