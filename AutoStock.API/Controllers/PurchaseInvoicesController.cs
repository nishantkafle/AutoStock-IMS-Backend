using AutoStock.Application.DTOs.PurchaseInvoices;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Only admin can manage purchase invoices
public class PurchaseInvoicesController(IPurchaseInvoiceService invoiceService) : ControllerBase
{
    private string GetAdminId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET api/purchaseinvoices?page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await invoiceService.GetAllInvoicesAsync(page, pageSize);
        return Ok(result);
    }

    // GET api/purchaseinvoices/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await invoiceService.GetInvoiceByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // POST api/purchaseinvoices - creates invoice and updates stock automatically
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseInvoiceRequestDto dto)
    {
        var result = await invoiceService.CreateInvoiceAsync(GetAdminId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
