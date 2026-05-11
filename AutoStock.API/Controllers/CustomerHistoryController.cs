using AutoStock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/customer/history")]
[Authorize(Roles = "Customer")] // only logged-in customers can call this
public class CustomerHistoryController : ControllerBase
{
    private readonly ICustomerHistoryService _historyService;

    public CustomerHistoryController(ICustomerHistoryService historyService)
    {
        _historyService = historyService;
    }

    // GET /api/customer/history
    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        // Get the current customer's ID from the JWT token
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(customerId))
            return Unauthorized(new { success = false, message = "User not found in token." });

        var data = await _historyService.GetHistoryAsync(customerId);

        return Ok(new { success = true, data });
    }
}
