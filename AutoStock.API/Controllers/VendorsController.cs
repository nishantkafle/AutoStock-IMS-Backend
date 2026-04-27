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
    public async Task<IActionResult> GetAllVendors()
    {
        var vendors = await _vendorService.GetAllVendorsAsync();
        return Ok(vendors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVendorById(Guid id)
    {
        var vendor = await _vendorService.GetVendorByIdAsync(id);
        if (vendor == null) return NotFound("Vendor not found");

        return Ok(vendor);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorDto dto)
    {
        var vendor = await _vendorService.CreateVendorAsync(dto);
        return CreatedAtAction(nameof(GetVendorById), new { id = vendor.Id }, vendor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(Guid id, [FromBody] UpdateVendorDto dto)
    {
        var result = await _vendorService.UpdateVendorAsync(id, dto);
        if (!result) return NotFound("Vendor not found");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendor(Guid id)
    {
        var result = await _vendorService.DeleteVendorAsync(id);
        if (!result) return NotFound("Vendor not found");

        return NoContent();
    }
}
