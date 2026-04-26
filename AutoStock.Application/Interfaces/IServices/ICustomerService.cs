using AutoStock.Application.DTOs.Customer;

namespace AutoStock.Application.Interfaces.IServices;

public interface ICustomerService
{
    //  Staff registers new customer with vehicle
    Task<ApiResponse<CustomerResponseDto>> RegisterCustomerAsync(CustomerRegisterDto dto);

    //  Staff views customer details, history, vehicle info
    Task<ApiResponse<CustomerResponseDto>> GetCustomerByIdAsync(string id);
    Task<ApiResponse<List<CustomerResponseDto>>> GetAllCustomersAsync();

    