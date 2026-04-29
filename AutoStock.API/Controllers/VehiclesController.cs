using AutoStock.Application.DTOs.Vehicle;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All vehicle endpoints require login
public class VehiclesController(IVehicleService vehicleService) : ControllerBaseusing AutoStock.Application.DTOs.Vehicle;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All vehicle endpoints require login
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    // Gets the logged-in user's ID from their JWT token
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET api/vehicles - customer sees their own vehicles
    [HttpGet]
    public async Task<IActionResult> GetMyVehicles()
    {
        var result = await vehicleService.GetMyVehiclesAsync(GetUserId());
        return Ok(result);
    }

    // POST api/vehicles - customer adds a vehicle 
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] VehicleRequestDto dto)
    {
        var result = await vehicleService.AddVehicleAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT api/vehicles/{id} - customer updates vehicle info
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleRequestDto dto)
    {
        var result = await vehicleService.UpdateVehicleAsync(GetUserId(), id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE api/vehicles/{id} - customer removes a vehicle
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await vehicleService.DeleteVehicleAsync(GetUserId(), id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

{
    // Gets the logged-in user's ID from their JWT token
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET api/vehicles - customer sees their own vehicles
    [HttpGet]
    public async Task<IActionResult> GetMyVehicles()
    {
        var result = await vehicleService.GetMyVehiclesAsync(GetUserId());
        return Ok(result);
    }

    // POST api/vehicles - customer adds a vehicle 
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] VehicleRequestDto dto)
    {
        var result = await vehicleService.AddVehicleAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT api/vehicles/{id} - customer updates vehicle info
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleRequestDto dto)
    {
        var result = await vehicleService.UpdateVehicleAsync(GetUserId(), id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE api/vehicles/{id} - customer removes a vehicle
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await vehicleService.DeleteVehicleAsync(GetUserId(), id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
