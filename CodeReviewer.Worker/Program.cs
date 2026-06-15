using CodeReviewer.Worker;
using CodeReviewer.Core.Interfaces;
using CodeReviewer.Infrastructure.Data;
using CodeReviewer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddHttpClient<GeminiReviewService>();
builder.Services.AddHostedService<ReviewWorker>();

var host = builder.Build();
host.Run();