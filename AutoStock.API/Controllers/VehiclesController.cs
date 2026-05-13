using AutoStock.Application.DTOs.Vehicle;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetMyVehicles()
    {
        var result = await vehicleService.GetMyVehiclesAsync(GetUserId());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] VehicleRequestDto dto)
    {
        var result = await vehicleService.AddVehicleAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleRequestDto dto)
    {
        var result = await vehicleService.UpdateVehicleAsync(GetUserId(), id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await vehicleService.DeleteVehicleAsync(GetUserId(), id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
