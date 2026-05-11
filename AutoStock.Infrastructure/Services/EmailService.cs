using AutoStock.Application.Interfaces.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutoStock.Infrastructure.Services;

public class EmailService(IOptions<EmailSettings> settings) : IEmailService
{
    private readonly EmailSettings _s = settings.Value;

    // Sends a 6-digit OTP to the given email address
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
}