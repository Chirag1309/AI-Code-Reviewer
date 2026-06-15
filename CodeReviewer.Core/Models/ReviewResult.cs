namespace CodeReviewer.Core.Models;

public class ReviewResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReviewRequestId { get; set; }
    public List<ReviewIssue> Issues { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public int QualityScore { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}