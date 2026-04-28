using AutoStock.Application.DTOs.Invoices;

namespace AutoStock.Application.Interfaces.IServices;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(string staffId, CreateInvoiceDto dto);
    Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync();
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid id);
}
