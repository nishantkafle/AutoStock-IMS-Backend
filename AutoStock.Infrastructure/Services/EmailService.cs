using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.Extensions.Configuration;

namespace AutoStock.Infrastructure.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendInvoiceEmailAsync(string toEmail, string customerName, Guid invoiceId, byte[] pdfAttachment)
    {
        var smtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = int.TryParse(configuration["EmailSettings:SmtpPort"], out var port) ? port : 587;
        var senderName = configuration["EmailSettings:SenderName"] ?? "AutoStock IMS";
        var senderEmail = configuration["EmailSettings:SenderEmail"];
        var password = configuration["EmailSettings:Password"];

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("[Email Service] SMTP Credentials missing. Skipping email.");
            return;
        }

        using var message = new MailMessage();
        message.From = new MailAddress(senderEmail, senderName);
        message.To.Add(new MailAddress(toEmail, customerName));
        message.Subject = $"Your Invoice #{invoiceId.ToString().Substring(0, 8)} from AutoStock";
        message.IsBodyHtml = true;
        message.Body = $"<p>Dear {customerName},</p><p>Please find attached your invoice for your recent purchase.</p><p>Thank you,<br/>AutoStock IMS</p>";

        if (pdfAttachment != null)
        {
            var ms = new MemoryStream(pdfAttachment);
            var attachment = new Attachment(ms, $"Invoice_{invoiceId.ToString().Substring(0, 8)}.pdf", "application/pdf");
            message.Attachments.Add(attachment);
        }

        using var smtp = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = true,
        };

        try
        {
            await smtp.SendMailAsync(message);
            Console.WriteLine($"[Email Service] Email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email via System.Net.Mail.SmtpClient: {ex.Message}");
        }
    }

    public async Task SendAppointmentEmailAsync(string toEmail, string customerName, string subject, string messageBody)
    {
        var smtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = int.TryParse(configuration["EmailSettings:SmtpPort"], out var port) ? port : 587;
        var senderName = configuration["EmailSettings:SenderName"] ?? "AutoStock IMS";
        var senderEmail = configuration["EmailSettings:SenderEmail"];
        var password = configuration["EmailSettings:Password"];

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("[Email Service] SMTP Credentials missing. Skipping email.");
            return;
        }

        using var message = new MailMessage();
        message.From = new MailAddress(senderEmail, senderName);
        message.To.Add(new MailAddress(toEmail, customerName));
        message.Subject = subject;
        message.IsBodyHtml = true;
        message.Body = $"<p>{messageBody.Replace("\n", "<br/>")}</p>";

        using var smtp = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = true,
        };

        try
        {
            await smtp.SendMailAsync(message);
            Console.WriteLine($"[Email Service] Email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email via System.Net.Mail.SmtpClient: {ex.Message}");
        }
    }
}
