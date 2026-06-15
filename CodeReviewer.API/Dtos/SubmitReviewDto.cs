using System.ComponentModel.DataAnnotations;

namespace CodeReviewer.API.Dtos;

public class SubmitReviewDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Language { get; set; } = string.Empty;

    [Url]
    public string? PrUrl { get; set; }
}
