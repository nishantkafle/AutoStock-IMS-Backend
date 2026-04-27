using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs.Reviews;

public class ReviewRequestDto
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;
}

public class ReviewResponseDto
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

