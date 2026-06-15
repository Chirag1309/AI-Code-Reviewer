using CodeReviewer.Core.Models;

namespace CodeReviewer.Core.Interfaces;

public interface IReviewService
{
    Task<ReviewRequest> CreateRequestAsync(string code, string language, string? prUrl = null);
    Task<ReviewRequest?> GetNextPendingAsync();
    Task UpdateStatusAsync(Guid id, ReviewStatus status);
    Task SaveResultAsync(ReviewResult result);
    Task<ReviewResult?> GetResultAsync(Guid requestId);
    Task<List<ReviewRequest>> GetAllAsync();
}