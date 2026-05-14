using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string CustomerPhone { get; set; } = string.Empty;

    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    // Staff user who created it
    public string StaffId { get; set; } = string.Empty;
    public User? Staff { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    public decimal SubTotal { get; set; }
    
    // Total discount applied
    public decimal DiscountAmount { get; set; }
    
    // Final total after discount
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string PaymentMethod { get; set; } = string.Empty;

    // Partial Payment fields
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }

    // Link to registered customer if applicable
    public string? CustomerId { get; set; }
    public User? Customer { get; set; }

    public ICollection<CreditSettlement> Settlements { get; set; } = new List<CreditSettlement>();
}
