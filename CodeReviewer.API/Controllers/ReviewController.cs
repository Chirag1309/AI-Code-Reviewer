using CodeReviewer.Core.Interfaces;
using CodeReviewer.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeReviewer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // POST api/review — submit code for review
    [HttpPost]
    public async Task<ActionResult<ReviewRequest>> SubmitReview(
        [FromBody] SubmitReviewDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest("Code cannot be empty");

        var request = await _reviewService.CreateRequestAsync(
            dto.Code, dto.Language, dto.PrUrl);

        return Accepted(request);
    }

    // GET api/review — get all reviews
    [HttpGet]
    public async Task<ActionResult<List<ReviewRequest>>> GetAll()
    {
        var reviews = await _reviewService.GetAllAsync();
        return Ok(reviews);
    }

    // GET api/review/{id} — get status + result for one review
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var all = await _reviewService.GetAllAsync();
        var request = all.FirstOrDefault(r => r.Id == id);

        if (request == null)
            return NotFound(new { message = $"Review {id} not found" });

        ReviewResult? result = null;

        if (request.Status == ReviewStatus.Completed)
            result = await _reviewService.GetResultAsync(id);

        return Ok(new { request, result });
    }
}

public class SubmitReviewDto
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = "csharp";
    public string? PrUrl { get; set; }
}