using CodeReviewer.Worker;
using CodeReviewer.Core.Interfaces;
using CodeReviewer.Infrastructure.Data;
using CodeReviewer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddHttpClient<GeminiReviewService>();
builder.Services.AddHostedService<ReviewWorker>();

var app = builder.Build();

// Minimal health check endpoint so Render sees the service as alive
app.MapGet("/", () => "Worker is running");

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();