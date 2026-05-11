using System.ComponentModel.DataAnnotations;

namespace AutoStock.Domain.Entities;

public class PurchaseInvoiceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PurchaseInvoiceId { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    // Which part was purchased
    public Guid PartId { get; set; }
    public Part? Part { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    // Calculated: Quantity x UnitCost
    public decimal Subtotal => Quantity * UnitCost;
}
