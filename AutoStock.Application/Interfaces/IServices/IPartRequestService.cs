using AutoStock.Application.DTOs.PartRequests;

namespace AutoStock.Application.Interfaces.IServices;

public interface IPartRequestService
{
    Task<ApiResponse<PartRequestResponseDto>> CreateRequestAsync(string customerId, PartRequestCreateDto dto);

    Task<ApiResponse<List<PartRequestResponseDto>>> GetMyRequestsAsync(string customerId);

    Task<ApiResponse<string>> DeleteRequestAsync(string customerId, Guid requestId);

    Task<ApiResponse<PagedResult<PartRequestResponseDto>>> GetAllRequestsAsync(int page, int pageSize);

    Task<ApiResponse<PartRequestResponseDto>> UpdateStatusAsync(Guid requestId, string status);

    Task<ApiResponse<PartRequestResponseDto>> PayRequestAsync(string customerId, Guid requestId);
}

