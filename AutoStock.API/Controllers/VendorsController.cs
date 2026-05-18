using AutoStock.Application.DTOs.Vendors;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _vendorService.GetAllVendorsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVendorById(Guid id)
    {
        var result = await _vendorService.GetVendorByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorDto dto)
    {
        var result = await _vendorService.CreateVendorAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(Guid id, [FromBody] UpdateVendorDto dto)
    {
        var result = await _vendorService.UpdateVendorAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendor(Guid id)
    {
        var result = await _vendorService.DeleteVendorAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
