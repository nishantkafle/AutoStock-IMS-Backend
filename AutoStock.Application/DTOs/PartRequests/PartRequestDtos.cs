using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.PartRequests;

public class PartRequestCreateDto
{
    [Required]
    public string PartName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Low / Medium / High
    public string Urgency { get; set; } = "Medium";
}

public class PartRequestResponseDto
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
