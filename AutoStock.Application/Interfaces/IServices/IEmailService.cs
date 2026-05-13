<<<<<<< HEAD
using System.Threading.Tasks;
=======
﻿using System;
using System.Collections.Generic;
using System.Text;
>>>>>>> 8ac8295763bb9c2ee4f81140895b1e42df3e2454

namespace AutoStock.Application.Interfaces.IServices;

public interface IEmailService
{
<<<<<<< HEAD
    Task SendInvoiceEmailAsync(string toEmail, string customerName, Guid invoiceId, byte[] pdfAttachment);
    Task SendAppointmentEmailAsync(string toEmail, string customerName, string subject, string messageBody);
}
=======
    Task SendOtpAsync(string toEmail, string otp);

    Task SendCredentialsEmailAsync(
        string toEmail,
        string toName,
        string password,
        string role,
        string? extraInfo = null
    );
}
>>>>>>> 8ac8295763bb9c2ee4f81140895b1e42df3e2454
