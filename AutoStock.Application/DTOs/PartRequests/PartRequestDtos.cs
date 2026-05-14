using System;
using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.PartRequests;

public class PartRequestCreateDto
{
    [Required]
    public Guid PartId { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public string Urgency { get; set; } = "Medium";
}

public class PartRequestResponseDto
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public Guid? PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

