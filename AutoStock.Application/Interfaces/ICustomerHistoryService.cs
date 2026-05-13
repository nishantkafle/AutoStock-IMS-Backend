using AutoStock.Application.DTOs;

namespace AutoStock.Application.Interfaces;

public interface ICustomerHistoryService
{
    // Returns all purchase invoices + appointments for the given customer
    Task<CustomerHistoryResponseDto> GetHistoryAsync(string customerId);
}
