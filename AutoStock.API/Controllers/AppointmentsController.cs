using AutoStock.Application.DTOs.Appointments;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // POST api/appointments - customer books an appointment
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Book([FromBody] AppointmentRequestDto dto)
    {
        var result = await appointmentService.BookAppointmentAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET api/appointments/mine - customer views own appointments
    [HttpGet("mine")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine()
    {
        var result = await appointmentService.GetMyAppointmentsAsync(GetUserId());
        return Ok(result);
    }

    // DELETE api/appointments/{id}/cancel - customer cancels
    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await appointmentService.CancelAppointmentAsync(GetUserId(), id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET api/appointments - admin and staff see all
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await appointmentService.GetAllAppointmentsAsync(page, pageSize);
        return Ok(result);
    }

    // PUT api/appointments/{id}/status - admin or staff updates status
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AppointmentStatusDto dto)
    {
        var result = await appointmentService.UpdateStatusAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
