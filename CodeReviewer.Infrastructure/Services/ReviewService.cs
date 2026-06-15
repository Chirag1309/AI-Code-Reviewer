using CodeReviewer.Core.Interfaces;
using CodeReviewer.Core.Models;
using CodeReviewer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeReviewer.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ReviewRequest> CreateRequestAsync(
        string code, string language, string? prUrl = null)
    {
        var request = new ReviewRequest
        {
            Code = code,
            Language = language,
            PrUrl = prUrl,
            Status = ReviewStatus.Pending
        };
        _db.ReviewRequests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<ReviewRequest?> GetNextPendingAsync()
    {
        return await _db.ReviewRequests
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateStatusAsync(Guid id, ReviewStatus status)
    {
        var request = await _db.ReviewRequests.FindAsync(id);
        if (request != null)
        {
            request.Status = status;
            await _db.SaveChangesAsync();
        }
    }

    public async Task SaveResultAsync(ReviewResult result)
    {
        _db.ReviewResults.Add(result);
        await _db.SaveChangesAsync();
    }

    public async Task<ReviewResult?> GetResultAsync(Guid requestId)
    {
        return await _db.ReviewResults
            .Include(r => r.Issues)
            .FirstOrDefaultAsync(r => r.ReviewRequestId == requestId);
    }

    public async Task<List<ReviewRequest>> GetAllAsync()
    {
        return await _db.ReviewRequests
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}