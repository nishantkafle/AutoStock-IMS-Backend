using System.Security.Claims;
using AutoStock.Application.DTOs.Invoices;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoStock.API.Controllers;

public class SendEmailRequestDto
{
    public string Email { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class InvoicesController(IInvoiceService invoiceService, IEmailService emailService) : ControllerBase
{
    static InvoicesController()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }
    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await invoiceService.CreateInvoiceAsync(userId, dto);
            return CreatedAtAction(nameof(GetInvoice), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllInvoices()
    {
        var result = await invoiceService.GetAllInvoicesAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        var result = await invoiceService.GetInvoiceByIdAsync(id);
        if (result == null) return NotFound(new { message = "Invoice not found" });

        return Ok(result);
    }

    [HttpPost("{id}/send-email")]
    public async Task<IActionResult> SendInvoiceEmail(Guid id, [FromBody] SendEmailRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest(new { message = "Email is required" });

        var invoice = await invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null) return NotFound(new { message = "Invoice not found" });

        try
        {
            var invoiceIdShort = invoice.Id.ToString().Substring(0, 8);
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica").FontColor(Colors.Black));

                    page.Content().Column(col =>
                    {
                        // 1. Header: Title and Date
                        col.Item().Text($"Invoice #{invoiceIdShort}").FontSize(22).ExtraBold();
                        col.Item().PaddingTop(4).PaddingBottom(32).Text($"Created on {invoice.CreatedAt:M/d/yyyy, h:mm:ss tt}").FontSize(11).FontColor(Colors.Grey.Darken1);

                        // 2. Main Content Box
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(24).Column(innerCol =>
                        {
                            // Billed To / Served By
                            innerCol.Item().PaddingBottom(24).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("BILLED TO").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Medium);
                                    c.Item().PaddingTop(6).Text(invoice.CustomerName).FontSize(14).Bold();
                                    c.Item().PaddingTop(2).Text(invoice.CustomerPhone).FontSize(11).FontColor(Colors.Grey.Darken2);
                                    if (!string.IsNullOrEmpty(invoice.CustomerAddress))
                                        c.Item().PaddingTop(2).Text($"Notes/VAT: {invoice.CustomerAddress}").FontSize(11).FontColor(Colors.Grey.Darken2);
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().AlignRight().Text("SERVED BY").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Medium);
                                    c.Item().AlignRight().PaddingTop(6).Text($"Staff ID: {invoice.StaffId.ToString().Substring(0, 8)}...").FontSize(11).Bold();
                                });
                            });

                            // Order Items Title
                            innerCol.Item().PaddingTop(24).PaddingBottom(16).Text("Order Items").FontSize(14).Bold();

                            // Items Table
                            innerCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(1.8f);
                                    columns.RelativeColumn(1.8f);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Lighten4).PaddingVertical(8).Text("Part Name").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Lighten4).PaddingVertical(8).AlignCenter().Text("Qty").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Lighten4).PaddingVertical(8).AlignRight().Text("Unit Price").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Lighten4).PaddingVertical(8).AlignRight().Text("Total").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                                });

                                foreach (var item in invoice.Items)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(12).Text(item.PartName).FontSize(12).Bold();
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(12).AlignCenter().Text(item.Quantity.ToString()).FontSize(11);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(12).AlignRight().Text($"NPR {item.UnitPrice:F2}").FontSize(11);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(12).AlignRight().Text($"NPR {item.TotalPrice:F2}").FontSize(12).Bold();
                                }
                            });

                            // Summary Section
                            innerCol.Item().PaddingTop(32).AlignRight().Width(240).Column(sCol =>
                            {
                                sCol.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Subtotal").FontSize(11).FontColor(Colors.Grey.Darken1);
                                    r.RelativeItem().AlignRight().Text($"NPR {invoice.SubTotal:F2}").FontSize(11);
                                });

                                if (invoice.DiscountAmount > 0)
                                {
                                    sCol.Item().PaddingTop(8).Row(r =>
                                    {
                                        r.RelativeItem().Text("Discount").FontSize(11).FontColor("#e11d48");
                                        r.RelativeItem().AlignRight().Text($"- NPR {invoice.DiscountAmount:F2}").FontSize(11).FontColor("#e11d48");
                                    });
                                }

                                sCol.Item().PaddingTop(12).BorderTop(2).BorderColor(Colors.Grey.Lighten4).PaddingTop(12).Row(r =>
                                {
                                    r.RelativeItem().Text("Total Amount").FontSize(16).ExtraBold();
                                    r.RelativeItem().AlignRight().Text($"NPR {invoice.TotalAmount:F2}").FontSize(16).ExtraBold();
                                });
                            });
                        });
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();

            await emailService.SendInvoiceEmailAsync(request.Email, invoice.CustomerName, invoice.Id, pdfBytes);

            return Ok(new { message = "Email sent successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to send email: " + ex.Message });
        }
    }

    [HttpPost("{id}/settle")]
    public async Task<IActionResult> SettleInvoice(Guid id, [FromBody] SettleInvoiceDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await invoiceService.SettleInvoiceAsync(id, userId, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(Guid id)
    {
        try
        {
            await invoiceService.DeleteInvoiceAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceDto dto)
    {
        try
        {
            var updated = await invoiceService.UpdateInvoiceAsync(id, dto);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/send-credit-reminder")]
    public async Task<IActionResult> SendCreditReminder(Guid id)
    {
        try
        {
            var invoice = await invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null) return NotFound();
            if (string.IsNullOrEmpty(invoice.CustomerEmail)) return BadRequest(new { message = "Customer email is missing." });

            await emailService.SendCreditReminderAsync(invoice.CustomerEmail, invoice.CustomerName, invoice.RemainingBalance);
            return Ok(new { message = "Credit reminder sent successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
