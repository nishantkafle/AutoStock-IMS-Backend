using AutoStock.Application.DTOs.Staff;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StaffController(IStaffService staffService) : ControllerBase
{
    // GET api/staff - list all staff
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await staffService.GetAllStaffAsync(page, pageSize);
        return Ok(result);
    }

    // GET api/staff/{id} - get one staff member
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await staffService.GetStaffByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // POST api/staff - admin creates a staff account
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StaffCreateDto dto)
    {
        var result = await staffService.CreateStaffAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT api/staff/{id} - admin edits staff name or active status
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] StaffUpdateDto dto)
    {
        var result = await staffService.UpdateStaffAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE api/staff/{id} - admin deactivates staff (soft delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var result = await staffService.DeactivateStaffAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
