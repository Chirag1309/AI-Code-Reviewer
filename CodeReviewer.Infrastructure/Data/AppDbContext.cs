using CodeReviewer.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeReviewer.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<ReviewRequest> ReviewRequests => Set<ReviewRequest>();
    public DbSet<ReviewResult> ReviewResults => Set<ReviewResult>();
    public DbSet<ReviewIssue> ReviewIssues => Set<ReviewIssue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReviewRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<ReviewResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Issues)
                  .WithOne()
                  .HasForeignKey(i => i.ReviewResultId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewIssue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Severity).HasMaxLength(20);
        });
    }
}