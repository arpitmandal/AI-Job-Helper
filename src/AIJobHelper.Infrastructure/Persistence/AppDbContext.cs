using AIJobHelper.Application.Interfaces;
using AIJobHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIJobHelper.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<ResumeAnalysis> ResumeAnalyses => Set<ResumeAnalysis>();
    public DbSet<JobDescription> JobDescriptions => Set<JobDescription>();
    public DbSet<AtsResult> AtsResults => Set<AtsResult>();
    public DbSet<CoverLetterConfig> CoverLetterConfigs => Set<CoverLetterConfig>();
    public DbSet<CoverLetter> CoverLetters => Set<CoverLetter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resume>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.FileName).HasMaxLength(512).IsRequired();
            e.Property(r => r.FileType).HasMaxLength(10).IsRequired();
            e.HasOne(r => r.Analysis).WithOne(a => a.Resume)
                .HasForeignKey<ResumeAnalysis>(a => a.ResumeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResumeAnalysis>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Summary).HasColumnType("longtext");
            e.Property(a => a.Strengths).HasColumnType("json");
            e.Property(a => a.Weaknesses).HasColumnType("json");
            e.Property(a => a.Suggestions).HasColumnType("json");
        });

        modelBuilder.Entity<JobDescription>(e =>
        {
            e.HasKey(j => j.Id);
            e.Property(j => j.SourceType).HasMaxLength(10).IsRequired();
            e.Property(j => j.Title).HasMaxLength(512);
            e.Property(j => j.RawContent).HasColumnType("longtext");
            e.Property(j => j.ParsedContent).HasColumnType("longtext");
        });

        modelBuilder.Entity<AtsResult>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Score).HasPrecision(5, 2);
            e.Property(a => a.MatchedSkills).HasColumnType("json");
            e.Property(a => a.MissingSkills).HasColumnType("json");
            e.Property(a => a.Suggestions).HasColumnType("json");
            e.HasOne(a => a.Resume).WithMany(r => r.AtsResults).HasForeignKey(a => a.ResumeId);
            e.HasOne(a => a.JobDescription).WithMany(j => j.AtsResults).HasForeignKey(a => a.JobDescriptionId);
        });

        modelBuilder.Entity<CoverLetterConfig>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(256).IsRequired();
            e.Property(c => c.Instructions).HasColumnType("longtext");
            e.Property(c => c.HeaderTemplate).HasColumnType("longtext");
            e.Property(c => c.FooterTemplate).HasColumnType("longtext");
        });

        modelBuilder.Entity<CoverLetter>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Content).HasColumnType("longtext");
            e.Property(c => c.FileName).HasMaxLength(512);
            e.Property(c => c.PdfPath).HasMaxLength(1024);
            e.HasOne(c => c.Resume).WithMany(r => r.CoverLetters).HasForeignKey(c => c.ResumeId);
            e.HasOne(c => c.JobDescription).WithMany(j => j.CoverLetters).HasForeignKey(c => c.JobDescriptionId);
            e.HasOne(c => c.Config).WithMany(cfg => cfg.CoverLetters).HasForeignKey(c => c.ConfigId);
        });
    }
}
