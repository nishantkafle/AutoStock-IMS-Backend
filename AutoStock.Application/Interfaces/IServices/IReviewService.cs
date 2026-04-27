using AutoStock.Application.DTOs.Reviews;

namespace AutoStock.Application.Interfaces.IServices;

public interface IReviewService
{
    // Customer submits a review
    Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(string customerId, ReviewRequestDto dto);

    // Anyone can view all reviews (public facing)
    Task<ApiResponse<List<ReviewResponseDto>>> GetAllReviewsAsync();

    // Customer views their own reviews
    Task<ApiResponse<List<ReviewResponseDto>>> GetMyReviewsAsync(string customerId);

    // Customer deletes their own review
    Task<ApiResponse<string>> DeleteReviewAsync(string customerId, Guid reviewId);
}
