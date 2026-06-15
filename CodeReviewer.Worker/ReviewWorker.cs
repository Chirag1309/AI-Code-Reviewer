using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CodeReviewer.Core.Interfaces;
using CodeReviewer.Core.Models;
using CodeReviewer.Infrastructure.Services;

namespace CodeReviewer.Worker;

public class ReviewWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReviewWorker> _logger;

    public ReviewWorker(IServiceScopeFactory scopeFactory,
                        ILogger<ReviewWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Review Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var reviewService = scope.ServiceProvider
                    .GetRequiredService<IReviewService>();
                var geminiService = scope.ServiceProvider
                    .GetRequiredService<GeminiReviewService>();

                var pending = await reviewService.GetNextPendingAsync();

                if (pending != null)
                {
                    _logger.LogInformation("Processing review {Id}", pending.Id);

                    await reviewService.UpdateStatusAsync(
                        pending.Id, ReviewStatus.Processing);

                    try
                    {
                        var result = await geminiService.ReviewCodeAsync(pending);
                        await reviewService.SaveResultAsync(result);
                        await reviewService.UpdateStatusAsync(
                            pending.Id, ReviewStatus.Completed);

                        _logger.LogInformation(
                            "Review {Id} completed. Score: {Score}",
                            pending.Id, result.QualityScore);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Review {Id} failed", pending.Id);
                        await reviewService.UpdateStatusAsync(
                            pending.Id, ReviewStatus.Failed);
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected worker error");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}