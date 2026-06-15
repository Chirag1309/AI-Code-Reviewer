namespace CodeReviewer.Core.Models;

public class ReviewIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReviewResultId { get; set; }        // ← add this FK
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public string? Suggestion { get; set; }
}