using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Vendors;

public record VendorDto(
    Guid Id,
    string Name,
    string ContactPerson,
    string Phone,
    string Email,
    string Address,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateVendorDto(
    [Required] string Name,
    string ContactPerson,
    string Phone,
    string Email,
    string Address
);

public record UpdateVendorDto(
    [Required] string Name,
    string ContactPerson,
    string Phone,
    string Email,
    string Address,
    bool IsActive
);
