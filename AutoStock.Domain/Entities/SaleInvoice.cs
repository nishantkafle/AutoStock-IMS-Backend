namespace AutoStock.Domain.Entities;

public class SaleInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Links to the customer (ApplicationUser)
    public string CustomerId { get; set; } = string.Empty;
    public User Customer { get; set; } = null!;

    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    // "Paid", "Unpaid", "Overdue"
    public string Status { get; set; } = "Unpaid";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property to line items
    public ICollection<SaleInvoiceItem> Items { get; set; } = new List<SaleInvoiceItem>();
}
