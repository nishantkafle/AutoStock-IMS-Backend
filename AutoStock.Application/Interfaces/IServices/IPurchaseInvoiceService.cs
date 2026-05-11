using AutoStock.Application.DTOs.PurchaseInvoices;

namespace AutoStock.Application.Interfaces.IServices;

public interface IPurchaseInvoiceService
{
    // Admin creates a purchase invoice - this also updates part stock quantities
    Task<ApiResponse<PurchaseInvoiceResponseDto>> CreateInvoiceAsync(string adminId, PurchaseInvoiceRequestDto dto);

    // Admin views all purchase invoices
    Task<ApiResponse<List<PurchaseInvoiceResponseDto>>> GetAllInvoicesAsync(int page, int pageSize);

    // Admin views a single invoice with all its items
    Task<ApiResponse<PurchaseInvoiceResponseDto>> GetInvoiceByIdAsync(Guid id);
}
