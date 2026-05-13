using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class CreditSettlement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public DateTime SettlementDate { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    // Staff who received the payment
    public string? StaffId { get; set; }
}
