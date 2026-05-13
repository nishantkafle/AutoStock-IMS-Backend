using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class PurchaseInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Vendor this stock was purchased from
    public Guid VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    // Admin who created this invoice
    [Required]
    public string CreatedByAdminId { get; set; } = string.Empty;
    public User? CreatedByAdmin { get; set; }

    [Required]
    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;

    // Line items for each part purchased
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = [];
}
