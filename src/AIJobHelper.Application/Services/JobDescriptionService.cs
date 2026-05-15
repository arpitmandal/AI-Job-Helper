using AIJobHelper.Application.DTOs.JobDescription;
using AIJobHelper.Application.Interfaces;
using AIJobHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIJobHelper.Application.Services;

public class JobDescriptionService : IJobDescriptionService
{
    private readonly IApplicationDbContext _db;
    private readonly IUrlContentFetcher _fetcher;
    private readonly ILogger<JobDescriptionService> _logger;

    public JobDescriptionService(
        IApplicationDbContext db,
        IUrlContentFetcher fetcher,
        ILogger<JobDescriptionService> logger)
    {
        _db = db;
        _fetcher = fetcher;
        _logger = logger;
    }

    public async Task<JobDescriptionDto> CreateAsync(string content, CancellationToken ct = default)
    {
        string sourceType;
        string rawContent;
        string parsedContent;

        if (content.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            content.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            sourceType = "url";
            rawContent = content;
            _logger.LogInformation("Fetching job description from URL {Url}", content);
            parsedContent = await _fetcher.FetchAsync(content, ct);
        }
        else
        {
            sourceType = "text";
            rawContent = content;
            parsedContent = content;
        }

        var jd = new JobDescription
        {
            SourceType = sourceType,
            RawContent = rawContent,
            ParsedContent = parsedContent
        };

        _db.JobDescriptions.Add(jd);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("JobDescription {Id} saved (source: {SourceType})", jd.Id, sourceType);
        return MapToDto(jd);
    }

    public async Task<JobDescriptionDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var jd = await _db.JobDescriptions.FirstOrDefaultAsync(j => j.Id == id, ct);
        return jd is null ? null : MapToDto(jd);
    }

    public async Task<IEnumerable<JobDescriptionDto>> ListAsync(CancellationToken ct = default)
    {
        var list = await _db.JobDescriptions
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(ct);

        return list.Select(MapToDto);
    }

    private static JobDescriptionDto MapToDto(JobDescription jd) => new()
    {
        Id = jd.Id,
        SourceType = jd.SourceType,
        ParsedContent = jd.ParsedContent,
        CreatedAt = jd.CreatedAt
    };
}
