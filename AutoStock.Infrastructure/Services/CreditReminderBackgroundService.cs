using AutoStock.Application.Interfaces.IServices;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStock.Infrastructure.Services;

public class CreditReminderBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<CreditReminderBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Credit Reminder Background Service has started.");

        // Initial delay on startup (e.g. 10 seconds) to let database settle
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Credit Reminder Background Service is running check...");
                await CheckAndSendCreditRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while running the credit reminder background check.");
            }

            // Check every 24 hours
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task CheckAndSendCreditRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        // Find invoices with:
        // 1. Outstanding credit (remaining balance > 0)
        // 2. Created more than 1 month ago
        // 3. Customer email is registered
        // 4. No reminder has been sent, or the last reminder was sent more than 30 days ago
        var invoices = await context.Invoices
            .Where(i => i.RemainingBalance > 0 
                     && i.CreatedAt < oneMonthAgo 
                     && !string.IsNullOrEmpty(i.CustomerEmail)
                     && (i.LastReminderSent == null || i.LastReminderSent < DateTime.UtcNow.AddDays(-30)))
            .ToListAsync(stoppingToken);

        logger.LogInformation("Found {Count} credit invoices matching the one-month overdue criteria.", invoices.Count);

        foreach (var invoice in invoices)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                logger.LogInformation("Sending automated credit reminder to {Email} for Invoice {Id} (Remaining: NPR {Balance})", 
                    invoice.CustomerEmail, invoice.Id, invoice.RemainingBalance);

                await emailService.SendCreditReminderAsync(invoice.CustomerEmail, invoice.CustomerName, invoice.RemainingBalance);

                // Update the last reminder sent date
                invoice.LastReminderSent = DateTime.UtcNow;
                context.Entry(invoice).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending credit reminder for invoice {Id} to {Email}", invoice.Id, invoice.CustomerEmail);
            }
        }

        if (invoices.Count > 0)
        {
            await context.SaveChangesAsync(stoppingToken);
            logger.LogInformation("Successfully completed and persisted automated credit reminders.");
        }
    }
}
