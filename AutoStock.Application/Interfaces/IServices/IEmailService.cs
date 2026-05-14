using System.Threading.Tasks;
using System;

namespace AutoStock.Application.Interfaces.IServices;

public interface IEmailService
{
    Task SendInvoiceEmailAsync(string toEmail, string customerName, Guid invoiceId, byte[] pdfAttachment);
    Task SendAppointmentEmailAsync(string toEmail, string customerName, string subject, string messageBody);
    Task SendOtpAsync(string toEmail, string otp);
    Task SendCredentialsEmailAsync(
        string toEmail,
        string toName,
        string password,
        string role,
        string? extraInfo = null
    );
    Task SendCreditReminderAsync(string toEmail, string customerName, decimal remainingBalance);
}
