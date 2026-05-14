using AutoStock.Application.DTOs.Reviews;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // POST api/reviews - customer submits a review
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] ReviewRequestDto dto)
    {
        var result = await reviewService.CreateReviewAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET api/reviews - all logged-in users can see all reviews
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await reviewService.GetAllReviewsAsync();
        return Ok(result);
    }

    // GET api/reviews/mine - customer sees only their own reviews
    [HttpGet("mine")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine()
    {
        var result = await reviewService.GetMyReviewsAsync(GetUserId());
        return Ok(result);
    }

    // DELETE api/reviews/{id} - customer deletes their own review
    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await reviewService.DeleteReviewAsync(GetUserId(), id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
