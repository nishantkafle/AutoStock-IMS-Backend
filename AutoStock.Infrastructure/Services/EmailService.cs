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

    // Sends login credentials to newly created Staff or Customer
    public async Task SendCredentialsEmailAsync(
        string toEmail,
        string toName,
        string password,
        string role,
        string? extraInfo = null)
    {
        // Extra vehicle row only shown for customers
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

                        <!-- Header -->
                        <tr>
                          <td style="background:#111111;padding:26px 36px;">
                            <span style="color:#ffffff;font-size:20px;font-weight:700;letter-spacing:-0.5px;">AutoStock</span>
                            <span style="color:#888;font-size:11px;margin-left:10px;letter-spacing:1px;text-transform:uppercase;">Parts Management</span>
                          </td>
                        </tr>

                        <!-- Body -->
                        <tr>
                          <td style="padding:36px 36px 28px;">
                            <h2 style="margin:0 0 8px;font-size:22px;font-weight:700;color:#111;letter-spacing:-0.5px;">
                              Welcome, {toName}!
                            </h2>
                            <p style="margin:0 0 24px;color:#666;font-size:14px;line-height:1.6;">
                              Your <strong>{role}</strong> account on AutoStock has been created.
                              Here are your login credentials.
                            </p>

                            <!-- Credentials box -->
                            <table width="100%" cellpadding="0" cellspacing="0"
                              style="border:1px solid #e8e8e6;border-radius:6px;overflow:hidden;margin-bottom:24px;">
                              <tr>
                                <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;color:#666;font-size:14px;width:110px;">Name</td>
                                <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;font-size:14px;font-weight:600;color:#111;">{toName}</td>
                              </tr>
                              <tr>
                                <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;color:#666;font-size:14px;">Email</td>
                                <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;font-size:14px;font-weight:600;color:#111;">{toEmail}</td>
                              </tr>
                              <tr>
                                <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;color:#666;font-size:14px;">Password</td>
                                <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;font-size:14px;font-weight:600;color:#111;font-family:monospace;letter-spacing:2px;">{password}</td>
                              </tr>
                              <tr>
                                <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;color:#666;font-size:14px;">Role</td>
                                <td style="padding:10px 14px;border-bottom:1px solid #f0f0f0;font-size:14px;color:#111;">{role}</td>
                              </tr>
                              {extraRow}
                            </table>

                            <!-- Warning -->
                            <table width="100%" cellpadding="0" cellspacing="0"
                              style="background:#fffbeb;border:1px solid #fde68a;border-radius:6px;margin-bottom:24px;">
                              <tr>
                                <td style="padding:14px 16px;font-size:13.5px;color:#92400e;line-height:1.6;">
                                  <strong>Important:</strong> Please log in and change your password immediately.<br>
                                  Go to <strong>Profile &rarr; Change Password</strong> after signing in.
                                </td>
                              </tr>
                            </table>

                            <p style="margin:0;font-size:14px;color:#666;line-height:1.6;">
                              If you have any issues, contact your AutoStock administrator.
                            </p>
                          </td>
                        </tr>

                        <!-- Footer -->
                        <tr>
                          <td style="background:#f8f8f6;padding:18px 36px;border-top:1px solid #ebebeb;">
                            <p style="margin:0;font-size:12px;color:#999;">
                              &copy; 2026 AutoStock &ndash; Professional Vehicle Parts Management.<br>
                              This is an automated message, please do not reply.
                            </p>
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
