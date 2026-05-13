using System.Threading.Tasks;

namespace AutoStock.Application.Interfaces.IServices;

public interface IEmailService
{
    Task SendInvoiceEmailAsync(string toEmail, string customerName, Guid invoiceId, byte[] pdfAttachment);
    Task SendAppointmentEmailAsync(string toEmail, string customerName, string subject, string messageBody);
}
