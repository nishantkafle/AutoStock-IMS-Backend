using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStock.Application.Interfaces.IServices;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string otp);

    Task SendCredentialsEmailAsync(
        string toEmail,
        string toName,
        string password,
        string role,
        string? extraInfo = null
    );
}