using AIJobHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIJobHelper.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Resume> Resumes { get; }
    DbSet<ResumeAnalysis> ResumeAnalyses { get; }
    DbSet<JobDescription> JobDescriptions { get; }
    DbSet<AtsResult> AtsResults { get; }
    DbSet<CoverLetterConfig> CoverLetterConfigs { get; }
    DbSet<CoverLetter> CoverLetters { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
