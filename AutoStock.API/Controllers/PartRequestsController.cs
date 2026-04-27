using AutoStock.Application.DTOs.PartRequests;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PartRequestsController(IPartRequestService partRequestService) : ControllerBase
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // POST api/partrequests - customer submits a part request
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] PartRequestCreateDto dto)
    {
        var result = await partRequestService.CreateRequestAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET api/partrequests/mine - customer views their own requests
    [HttpGet("mine")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine()
    {
        var result = await partRequestService.GetMyRequestsAsync(GetUserId());
        return Ok(result);
    }

    // DELETE api/partrequests/{id} - customer deletes a pending request
    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await partRequestService.DeleteRequestAsync(GetUserId(), id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // GET api/partrequests - admin and staff see all requests
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll()
    {
        var result = await partRequestService.GetAllRequestsAsync();
        return Ok(result);
    }

    // PUT api/partrequests/{id}/status - admin or staff updates status
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status)
    {
        var result = await partRequestService.UpdateStatusAsync(id, status);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
