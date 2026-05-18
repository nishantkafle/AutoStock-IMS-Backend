using AutoStock.Application.DTOs.Invoices;

namespace AutoStock.Application.Interfaces.IServices;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(string staffId, CreateInvoiceDto dto);
    Task<PagedResult<InvoiceResponseDto>> GetAllInvoicesAsync(int page, int pageSize, string? paymentMethod = null);
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid id);
    Task<InvoiceResponseDto> SettleInvoiceAsync(Guid id, string staffId, SettleInvoiceDto dto);
    Task DeleteInvoiceAsync(Guid id);
    Task<InvoiceResponseDto> UpdateInvoiceAsync(Guid id, UpdateInvoiceDto dto);
    Task UpdateLastReminderSentAsync(Guid id, DateTime date);
}
