using CodeReviewer.Core.Interfaces;
using CodeReviewer.Infrastructure.Data;
using CodeReviewer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — must be registered here before builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")));

// App services
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddHttpClient<GeminiReviewService>();

// ── Pipeline ─────────────────────────────────────────────
var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Urls.Add($"http://0.0.0.0:{port}");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");      // before MapControllers
app.UseAuthorization();
app.MapControllers();

app.Run();