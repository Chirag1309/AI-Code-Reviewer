using System.Text.Json;
using CodeReviewer.Core.Models;
using Microsoft.Extensions.Configuration;

namespace CodeReviewer.Infrastructure.Services;

public class GeminiReviewService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://generativelanguage.googleapis.com";

    public GeminiReviewService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"]!;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<ReviewResult> ReviewCodeAsync(ReviewRequest request)
    {
        var prompt = BuildPrompt(request.Code, request.Language);

        var payload = new
        {
            contents = new[]
            {
            new { parts = new[] { new { text = prompt } } }
        },
            generationConfig = new { temperature = 0.1, maxOutputTokens = 4096 }
        };

        var json = JsonSerializer.Serialize(payload);
        var url = $"/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        // Retry up to 3 times with delay
        int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var rawText = ExtractText(responseBody);
                Console.WriteLine("=== GEMINI RAW RESPONSE ===\n" + rawText);
                return ParseReviewResponse(rawText, request.Id);
            }

            if ((int)response.StatusCode == 503 && attempt < maxRetries)
            {
                Console.WriteLine($"Gemini 503 — retrying attempt {attempt + 1} in {attempt * 3}s...");
                await Task.Delay(TimeSpan.FromSeconds(attempt * 3));
                continue;
            }

            // Non-retryable error
            response.EnsureSuccessStatusCode();
        }

        // Should never reach here
        throw new Exception("Gemini API failed after all retries");
    }
    private string ExtractText(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";
        }
        catch
        {
            return "";
        }
    }

    private string BuildPrompt(string code, string language)
    {
        return $$"""
    You are an expert code reviewer. Analyze the following {{language}} code.
    Return ONLY a valid JSON object. No explanation, no markdown, no code fences.

    Code to review:
    {{code}}

    Return exactly this JSON structure:
    {
        "summary": "brief overall assessment",
        "qualityScore": 75,
        "issues": [
            {
                "category": "Security",
                "severity": "High",
                "description": "description of issue",
                "lineNumber": 5,
                "suggestion": "how to fix it"
            }
        ]
    }

    Categories must be one of: Security, Performance, Maintainability, Bug, BestPractice
    Severity must be one of: Critical, High, Medium, Low
    Return only the JSON, nothing else.
    """;
    }

    private ReviewResult ParseReviewResponse(string rawText, Guid requestId)
    {

        try
        {
            // Strip all possible markdown variations
            var clean = rawText
                .Replace("```json", "")
                .Replace("```JSON", "")
                .Replace("```", "")
                .Trim();
            Console.WriteLine("=== AFTER STRIP ===");
            Console.WriteLine(clean);
            // Find the first { and last } to extract pure JSON
            int start = clean.IndexOf('{');
            int end = clean.LastIndexOf('}');

            if (start == -1 || end == -1)
            {
                Console.WriteLine("=== TRUNCATED RESPONSE — likely hit token limit ===");
                return new ReviewResult
                {
                    ReviewRequestId = requestId,
                    Summary = "AI response was truncated. Try with shorter code or increase token limit.",
                    QualityScore = 0,
                    Issues = new()
                };
            }

            clean = clean.Substring(start, end - start + 1);

            Console.WriteLine("=== FINAL JSON TO PARSE ===");
            Console.WriteLine(clean);

            using var doc = JsonDocument.Parse(clean);
            var root = doc.RootElement;

            var issues = new List<ReviewIssue>();

            if (root.TryGetProperty("issues", out var issuesEl))
            {
                foreach (var issue in issuesEl.EnumerateArray())
                {
                    issues.Add(new ReviewIssue
                    {
                        Category = issue.TryGetProperty("category", out var cat)
                            ? cat.GetString() ?? "" : "",
                        Severity = issue.TryGetProperty("severity", out var sev)
                            ? sev.GetString() ?? "" : "",
                        Description = issue.TryGetProperty("description", out var desc)
                            ? desc.GetString() ?? "" : "",
                        LineNumber = issue.TryGetProperty("lineNumber", out var line)
                            && line.ValueKind == JsonValueKind.Number
                            ? line.GetInt32() : null,
                        Suggestion = issue.TryGetProperty("suggestion", out var sug)
                            ? sug.GetString() : null
                    });
                }
            }

            return new ReviewResult
            {
                ReviewRequestId = requestId,
                Summary = root.TryGetProperty("summary", out var sumEl)
                    ? sumEl.GetString() ?? "" : "",
                QualityScore = root.TryGetProperty("qualityScore", out var scoreEl)
                    ? scoreEl.GetInt32() : 0,
                Issues = issues
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== PARSE EXCEPTION ===");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            return new ReviewResult
            {
                ReviewRequestId = requestId,
                Summary = "Failed to parse AI response",
                QualityScore = 0,
                Issues = new()
            };
        }
    }
}