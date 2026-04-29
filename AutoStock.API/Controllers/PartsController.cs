using AutoStock.Application.DTOs.Parts;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController(IPartService partService) : ControllerBase
{
    // Anyone logged in can view parts - customers and staff need to see inventory
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await partService.GetAllPartsAsync(page, pageSize);
        return Ok(result);
    }

    // Anyone logged in can view a single part
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await partService.GetPartByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Only admin can create parts 
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] PartRequestDto dto)
    {
        var result = await partService.CreatePartAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Only admin can edit parts 
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PartRequestDto dto)
    {
        var result = await partService.UpdatePartAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Only admin can delete parts
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await partService.DeletePartAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Admin gets list of low stock parts for dashboard alerts 
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLowStock()
    {
        var result = await partService.GetLowStockPartsAsync();
        return Ok(result);
    }
}
