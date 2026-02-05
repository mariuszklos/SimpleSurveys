using Microsoft.EntityFrameworkCore;
using SimpleSurveys.Api.Models;

namespace SimpleSurveys.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyOption> SurveyOptions => Set<SurveyOption>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.SelectionMode)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<SurveyOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OptionType)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(e => e.TextValue).HasMaxLength(500);

            entity.HasOne(e => e.Survey)
                .WithMany(s => s.Options)
                .HasForeignKey(e => e.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VoterToken).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Survey)
                .WithMany(s => s.Votes)
                .HasForeignKey(e => e.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Option)
                .WithMany(o => o.Votes)
                .HasForeignKey(e => e.OptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SurveyId, e.VoterToken });
        });
    }
}
