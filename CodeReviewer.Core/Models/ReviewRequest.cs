namespace CodeReviewer.Core.Models;

public class ReviewRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? PrUrl { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReviewStatus 
{ 
    Pending, 
    Processing, 
    Completed, 
    Failed 
}