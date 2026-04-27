using AutoStock.Application.DTOs.PartRequests;

namespace AutoStock.Application.Interfaces.IServices;

public interface IPartRequestService
{
    // Customer submits a request for a part not in stock
    Task<ApiResponse<PartRequestResponseDto>> CreateRequestAsync(string customerId, PartRequestCreateDto dto);

    // Customer views their own requests
    Task<ApiResponse<List<PartRequestResponseDto>>> GetMyRequestsAsync(string customerId);

    // Customer deletes a pending request
    Task<ApiResponse<string>> DeleteRequestAsync(string customerId, Guid requestId);

    // Admin or staff views all requests
    Task<ApiResponse<List<PartRequestResponseDto>>> GetAllRequestsAsync();

    // Admin or staff marks a request as Fulfilled or Rejected
    Task<ApiResponse<PartRequestResponseDto>> UpdateStatusAsync(Guid requestId, string status);
}
