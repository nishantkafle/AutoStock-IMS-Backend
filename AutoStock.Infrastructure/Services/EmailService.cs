using AutoStock.Application.Interfaces.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.IO;
using System.Threading.Tasks;
using AutoStock.Application;

namespace AutoStock.Infrastructure.Services;

public class EmailService(IOptions<EmailSettings> settings) : IEmailService
{
    private readonly EmailSettings _s = settings.Value;

    public async Task SendInvoiceEmailAsync(string toEmail, string customerName, Guid invoiceId, byte[] pdfAttachment)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_s.SenderName, _s.SenderEmail));
        message.To.Add(new MailboxAddress(customerName, toEmail));
        message.Subject = $"Your Invoice #{invoiceId.ToString().Substring(0, 8)} from AutoStock";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $"<p>Dear {customerName},</p><p>Please find attached your invoice for your recent purchase.</p><p>Thank you,<br/>AutoStock IMS</p>"
        };

        if (pdfAttachment != null)
        {
            bodyBuilder.Attachments.Add($"Invoice_{invoiceId.ToString().Substring(0, 8)}.pdf", pdfAttachment, ContentType.Parse("application/pdf"));
        }

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_s.SmtpHost, _s.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_s.BrevoLogin, _s.AppPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendAppointmentEmailAsync(string toEmail, string customerName, string subject, string messageBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_s.SenderName, _s.SenderEmail));
        message.To.Add(new MailboxAddress(customerName, toEmail));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = $"<p>{messageBody.Replace("\n", "<br/>")}</p>"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_s.SmtpHost, _s.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_s.BrevoLogin, _s.AppPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_s.SenderName, _s.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "AutoStock - Email Verification Code";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <h2>Verify your email</h2>
                <p>Your verification code is:</p>
                <h1 style="letter-spacing:8px">{otp}</h1>
                <p>This code expires in 10 minutes.</p>
            """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_s.SmtpHost, _s.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_s.BrevoLogin, _s.AppPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendCredentialsEmailAsync(
        string toEmail,
        string toName,
        string password,
        string role,
        string? extraInfo = null)
    {
        string extraRow = string.IsNullOrEmpty(extraInfo) ? "" : $"""
            <tr>
              <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;color:#666;font-size:14px;width:110px;">Vehicle</td>
              <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;font-size:14px;color:#111;">{extraInfo}</td>
            </tr>
        """;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_s.SenderName, _s.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"Welcome to AutoStock - Your {role} Account";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <!DOCTYPE html>
                <html>
                <head><meta charset="UTF-8"></head>
                <body style="margin:0;padding:0;background:#f2f2f0;font-family:Arial,sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" style="padding:48px 0;">
                    <tr><td align="center">
                      <table width="540" cellpadding="0" cellspacing="0"
                        style="background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e0e0dc;">
                        <tr>
                          <td style="background:#111111;padding:26px 36px;">
                            <span style="color:#ffffff;font-size:20px;font-weight:700;letter-spacing:-0.5px;">AutoStock</span>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:36px 36px 28px;">
                            <h2 style="margin:0 0 8px;font-size:22px;font-weight:700;color:#111;">Welcome, {toName}!</h2>
                            <p>Your {role} account has been created.</p>
                            <table width="100%" border="1" cellpadding="10" cellspacing="0" style="border-collapse:collapse;">
                              <tr><td>Email</td><td>{toEmail}</td></tr>
                              <tr><td>Password</td><td>{password}</td></tr>
                              {extraRow}
                            </table>
                          </td>
                        </tr>
                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
            """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_s.SmtpHost, _s.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_s.BrevoLogin, _s.AppPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
