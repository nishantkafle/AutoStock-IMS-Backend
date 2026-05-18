using AutoStock.Application;
using AutoStock.Application.DTOs.Reviews;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using AutoStock.Infrastructure.Extensions;
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
        PartRequestId = r.PartRequestId,
        PartName = r.PartRequest?.PartName ?? "",
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };

    public async Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(
        string customerId, ReviewRequestDto dto)
    {
        if (dto.PartRequestId.HasValue)
        {
            var partRequest = await db.PartRequests
                .FirstOrDefaultAsync(pr => pr.Id == dto.PartRequestId && pr.CustomerId == customerId);

            if (partRequest == null)
                return ApiResponse<ReviewResponseDto>.Fail("Part request not found");

            if (!partRequest.IsPaid || partRequest.Status != "Fulfilled")
                return ApiResponse<ReviewResponseDto>.Fail("You can only review fulfilled and paid part requests");

            var alreadyReviewed = await db.Reviews
                .AnyAsync(r => r.PartRequestId == dto.PartRequestId && r.CustomerId == customerId);

            if (alreadyReviewed)
                return ApiResponse<ReviewResponseDto>.Fail("You have already reviewed this part request");
        }

        var review = new Review
        {
            CustomerId = customerId,
            PartRequestId = dto.PartRequestId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();

        await db.Entry(review).Reference(r => r.Customer).LoadAsync();
        if (review.PartRequestId.HasValue)
            await db.Entry(review).Reference(r => r.PartRequest).LoadAsync();

        logger.LogInformation("Review submitted by customer {Id} with rating {Rating}", customerId, dto.Rating);
        return ApiResponse<ReviewResponseDto>.Ok(ToDto(review), "Review submitted");
    }

    public async Task<ApiResponse<PagedResult<ReviewResponseDto>>> GetAllReviewsAsync(int page, int pageSize)
    {
        var query = db.Reviews
            .Include(r => r.Customer)
            .Include(r => r.PartRequest)
            .OrderByDescending(r => r.CreatedAt);

        var pagedReviews = await query.ToPagedResultAsync(page, pageSize);
        var mappedItems = pagedReviews.Items.Select(ToDto).ToList();
        var result = new PagedResult<ReviewResponseDto>(mappedItems, pagedReviews.TotalCount, pagedReviews.PageNumber, pagedReviews.PageSize);

        return ApiResponse<PagedResult<ReviewResponseDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<ReviewResponseDto>>> GetMyReviewsAsync(string customerId)
    {
        var list = await db.Reviews
            .Include(r => r.Customer)
            .Include(r => r.PartRequest)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ReviewResponseDto>>.Ok(list.Select(ToDto).ToList());
    }

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
