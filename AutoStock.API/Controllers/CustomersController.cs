using AutoStock.Application.DTOs.Customer;
using AutoStock.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    //  Staff registers new customer with vehicle details
    // POST api/customers/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterDto dto)
    {
        var result = await _customerService.RegisterCustomerAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    //  Staff views all customers
    // GET api/customers
    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var result = await _customerService.GetAllCustomersAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    //  Staff views specific customer details and vehicle info
    // GET api/customers/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(string id)
    {
        var result = await _customerService.GetCustomerByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
