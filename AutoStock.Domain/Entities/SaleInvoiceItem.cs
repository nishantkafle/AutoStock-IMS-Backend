namespace AutoStock.Domain.Entities;

public class SaleInvoiceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SaleInvoiceId { get; set; }
    public SaleInvoice SaleInvoice { get; set; } = null!;

    // Part details at time of sale
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
