using AutoStock.Application;
using AutoStock.Application.DTOs.PartRequests;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class PartRequestService(
    AppDbContext db,
    ILogger<PartRequestService> logger
) : IPartRequestService
{
    private static PartRequestResponseDto ToDto(PartRequest r) => new()
    {
        Id = r.Id,
        CustomerId = r.CustomerId,
        CustomerName = r.Customer?.FullName ?? "",
        PartName = r.PartName,
        Description = r.Description,
        Urgency = r.Urgency,
        Status = r.Status,
        RequestedAt = r.RequestedAt,
        ResolvedAt = r.ResolvedAt
    };

    public async Task<ApiResponse<PartRequestResponseDto>> CreateRequestAsync(
        string customerId, PartRequestCreateDto dto)
    {
        var request = new PartRequest
        {
            CustomerId = customerId,
            PartName = dto.PartName,
            Description = dto.Description,
            Urgency = dto.Urgency,
            Status = "Pending"
        };

        db.PartRequests.Add(request);
        await db.SaveChangesAsync();

        await db.Entry(request).Reference(r => r.Customer).LoadAsync();

        logger.LogInformation("Part request created by {CustomerId} for {PartName}", customerId, dto.PartName);
        return ApiResponse<PartRequestResponseDto>.Ok(ToDto(request), "Part request submitted");
    }

    public async Task<ApiResponse<List<PartRequestResponseDto>>> GetMyRequestsAsync(string customerId)
    {
        var list = await db.PartRequests
            .Include(r => r.Customer)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return ApiResponse<List<PartRequestResponseDto>>.Ok(list.Select(ToDto).ToList());
    }

    // Customer can only delete their own pending requests
    public async Task<ApiResponse<string>> DeleteRequestAsync(string customerId, Guid requestId)
    {
        var request = await db.PartRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.CustomerId == customerId);

        if (request == null)
            return ApiResponse<string>.Fail("Request not found");

        if (request.Status != "Pending")
            return ApiResponse<string>.Fail("Only pending requests can be deleted");

        db.PartRequests.Remove(request);
        await db.SaveChangesAsync();
        return ApiResponse<string>.Ok("Request deleted");
    }

    public async Task<ApiResponse<List<PartRequestResponseDto>>> GetAllRequestsAsync()
    {
        var list = await db.PartRequests
            .Include(r => r.Customer)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return ApiResponse<List<PartRequestResponseDto>>.Ok(list.Select(ToDto).ToList());
    }

    // Admin or staff fulfils or rejects a request
    public async Task<ApiResponse<PartRequestResponseDto>> UpdateStatusAsync(Guid requestId, string status)
    {
        var allowed = new[] { "Pending", "Fulfilled", "Rejected" };
        if (!allowed.Contains(status))
            return ApiResponse<PartRequestResponseDto>.Fail("Invalid status");

        var request = await db.PartRequests
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            return ApiResponse<PartRequestResponseDto>.Fail("Request not found");

        request.Status = status;
        if (status != "Pending")
            request.ResolvedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Part request {Id} marked as {Status}", requestId, status);
        return ApiResponse<PartRequestResponseDto>.Ok(ToDto(request), "Status updated");
    }
}
