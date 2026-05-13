<<<<<<< HEAD
using AutoStock.Application.DTOs.Vehicle;
=======
﻿using AutoStock.Application.DTOs.Vehicle;
>>>>>>> 8ac8295763bb9c2ee4f81140895b1e42df3e2454
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
<<<<<<< HEAD
[Authorize] // All vehicle endpoints require login
=======
[Authorize]
>>>>>>> 8ac8295763bb9c2ee4f81140895b1e42df3e2454
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
<<<<<<< HEAD
}
=======
}
>>>>>>> 8ac8295763bb9c2ee4f81140895b1e42df3e2454
