using System.Security.Claims;
using AutoStock.Application.DTOs.Invoices;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
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
}
