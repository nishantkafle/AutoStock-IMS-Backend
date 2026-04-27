using AutoStock.Application;
using AutoStock.Application.DTOs.Reviews;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class ReviewService(
    AppDbContext db,
    ILogger<ReviewService> logger
) : IReviewService
{
    private static ReviewResponseDto ToDto(Review r) => new()
    {
        Id = r.Id,
        CustomerId = r.CustomerId,
        CustomerName = r.Customer?.FullName ?? "",
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };

    public async Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(
        string customerId, ReviewRequestDto dto)
    {
        var review = new Review
        {
            CustomerId = customerId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();

        await db.Entry(review).Reference(r => r.Customer).LoadAsync();

        logger.LogInformation("Review submitted by customer {Id} with rating {Rating}", customerId, dto.Rating);
        return ApiResponse<ReviewResponseDto>.Ok(ToDto(review), "Review submitted");
    }

    // All reviews are public - anyone logged in can see them
    public async Task<ApiResponse<List<ReviewResponseDto>>> GetAllReviewsAsync()
    {
        var list = await db.Reviews
            .Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ReviewResponseDto>>.Ok(list.Select(ToDto).ToList());
    }

    public async Task<ApiResponse<List<ReviewResponseDto>>> GetMyReviewsAsync(string customerId)
    {
        var list = await db.Reviews
            .Include(r => r.Customer)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ReviewResponseDto>>.Ok(list.Select(ToDto).ToList());
    }

    // Customer can delete their own review
    public async Task<ApiResponse<string>> DeleteReviewAsync(string customerId, Guid reviewId)
    {
        var review = await db.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.CustomerId == customerId);

        if (review == null)
            return ApiResponse<string>.Fail("Review not found");

        db.Reviews.Remove(review);
        await db.SaveChangesAsync();
        return ApiResponse<string>.Ok("Review deleted");
    }
}
