using AutoStock.Application;
using AutoStock.Application.DTOs.PartRequests;
using AutoStock.Application.Interfaces.IServices;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using AutoStock.Infrastructure.Extensions;
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
        PartId = r.PartId,
        PartName = r.Part?.Name ?? r.PartName,
        Price = r.Part?.Price ?? 0,
        Quantity = r.Quantity,
        Description = r.Description,
        Urgency = r.Urgency,
        Status = r.Status,
        IsPaid = r.IsPaid,
        RequestedAt = r.RequestedAt,
        ResolvedAt = r.ResolvedAt
    };

    public async Task<ApiResponse<PartRequestResponseDto>> CreateRequestAsync(
        string customerId, PartRequestCreateDto dto)
    {
        var part = await db.Parts.FindAsync(dto.PartId);
        if (part == null)
            return ApiResponse<PartRequestResponseDto>.Fail("Part not found");

        if (part.StockQty < dto.Quantity)
            return ApiResponse<PartRequestResponseDto>.Fail("Not enough stock available");

        var request = new PartRequest
        {
            CustomerId = customerId,
            PartId = dto.PartId,
            PartName = part.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Urgency = dto.Urgency,
            Status = "Pending"
        };

        db.PartRequests.Add(request);
        await db.SaveChangesAsync();

        await db.Entry(request).Reference(r => r.Customer).LoadAsync();
        await db.Entry(request).Reference(r => r.Part).LoadAsync();

        logger.LogInformation("Part request created by {CustomerId} for {PartName}", customerId, part.Name);
        return ApiResponse<PartRequestResponseDto>.Ok(ToDto(request), "Part request submitted");
    }

    public async Task<ApiResponse<List<PartRequestResponseDto>>> GetMyRequestsAsync(string customerId)
    {
        var list = await db.PartRequests
            .Include(r => r.Customer)
            .Include(r => r.Part)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return ApiResponse<List<PartRequestResponseDto>>.Ok(list.Select(ToDto).ToList());
    }

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

    public async Task<ApiResponse<PagedResult<PartRequestResponseDto>>> GetAllRequestsAsync(int page, int pageSize)
    {
        var query = db.PartRequests
            .Include(r => r.Customer)
            .Include(r => r.Part)
            .OrderByDescending(r => r.RequestedAt);

        var pagedRequests = await query.ToPagedResultAsync(page, pageSize);
        var mappedItems = pagedRequests.Items.Select(ToDto).ToList();
        var result = new PagedResult<PartRequestResponseDto>(mappedItems, pagedRequests.TotalCount, pagedRequests.PageNumber, pagedRequests.PageSize);

        return ApiResponse<PagedResult<PartRequestResponseDto>>.Ok(result);
    }

    public async Task<ApiResponse<PartRequestResponseDto>> UpdateStatusAsync(Guid requestId, string status)
    {
        var allowed = new[] { "Pending", "Fulfilled", "Rejected" };
        if (!allowed.Contains(status))
            return ApiResponse<PartRequestResponseDto>.Fail("Invalid status");

        var request = await db.PartRequests
            .Include(r => r.Customer)
            .Include(r => r.Part)
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

    public async Task<ApiResponse<PartRequestResponseDto>> PayRequestAsync(string customerId, Guid requestId)
    {
        var request = await db.PartRequests
            .Include(r => r.Customer)
            .Include(r => r.Part)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.CustomerId == customerId);

        if (request == null)
            return ApiResponse<PartRequestResponseDto>.Fail("Request not found");

        if (request.IsPaid)
            return ApiResponse<PartRequestResponseDto>.Fail("Already paid");

        if (request.Status != "Fulfilled")
            return ApiResponse<PartRequestResponseDto>.Fail("Request must be fulfilled by admin before payment");

        if (request.Part == null)
            return ApiResponse<PartRequestResponseDto>.Fail("Part not found");

        if (request.Part.StockQty < request.Quantity)
            return ApiResponse<PartRequestResponseDto>.Fail("Not enough stock available");

        var totalCost = request.Part.Price * request.Quantity;

        request.Part.StockQty -= request.Quantity;
        request.Part.UpdatedAt = DateTime.UtcNow;
        request.IsPaid = true;

        await db.SaveChangesAsync();
        logger.LogInformation("Part request {Id} paid, stock decreased by {Qty}", requestId, request.Quantity);
        
        var dtoResponse = ToDto(request);

        // Loyalty Program: 10% discount if total cost > 5000
        if (totalCost > 5000)
        {
            var discount = totalCost * 0.10m;
            dtoResponse.Price = request.Part.Price - (discount / request.Quantity); // Adjust the price returned to reflect discount
            logger.LogInformation("Loyalty discount of {Discount} applied to part request {Id}", discount, requestId);
        }

        return ApiResponse<PartRequestResponseDto>.Ok(dtoResponse, "Payment successful, stock updated");
    }
}

