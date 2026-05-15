using System.Text.Json;
using AIJobHelper.Application.DTOs.JobDescription;
using AIJobHelper.Application.Interfaces;
using AIJobHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIJobHelper.Application.Services;

public class JobDescriptionService : IJobDescriptionService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IApplicationDbContext _db;
    private readonly IUrlContentFetcher _fetcher;
    private readonly IGeminiClient _gemini;
    private readonly ILogger<JobDescriptionService> _logger;

    public JobDescriptionService(
        IApplicationDbContext db,
        IUrlContentFetcher fetcher,
        IGeminiClient gemini,
        ILogger<JobDescriptionService> logger)
    {
        _db = db;
        _fetcher = fetcher;
        _gemini = gemini;
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

        var title = await ExtractTitleAsync(parsedContent, ct);

        var jd = new JobDescription
        {
            SourceType = sourceType,
            Title = title,
            RawContent = rawContent,
            ParsedContent = parsedContent
        };

        _db.JobDescriptions.Add(jd);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("JobDescription {Id} saved — \"{Title}\"", jd.Id, title);
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

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var jd = await _db.JobDescriptions
            .Include(j => j.AtsResults)
            .Include(j => j.CoverLetters)
            .FirstOrDefaultAsync(j => j.Id == id, ct)
            ?? throw new KeyNotFoundException($"Job description {id} not found.");

        // AtsResult.JobDescriptionId is non-nullable — cascade remove them
        _db.AtsResults.RemoveRange(jd.AtsResults);

        // CoverLetter.JobDescriptionId is nullable — unlink rather than delete
        foreach (var cl in jd.CoverLetters)
            cl.JobDescriptionId = null;

        _db.JobDescriptions.Remove(jd);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("JobDescription {Id} deleted ({AtsCount} ATS results removed)", id, jd.AtsResults.Count);
    }

    private async Task<string> ExtractTitleAsync(string text, CancellationToken ct)
    {
        try
        {
            // Send only first 2000 chars to keep the prompt small
            var snippet = text.Length > 2000 ? text[..2000] : text;

            var prompt = $$"""
                From the job description below, extract: company name, job title, and location (city or Remote).
                Respond with ONLY valid JSON — no markdown, no code blocks:
                {"company":"...","jobTitle":"...","location":"..."}
                Use "Unknown" if a field cannot be determined. Be concise — just the name/title/city, no extra words.

                Job Description:
                {{snippet}}
                """;

            var raw = await _gemini.GenerateAsync(prompt, ct);
            var json = raw.Trim();
            if (json.StartsWith("```")) { json = json[(json.IndexOf('\n') + 1)..]; json = json[..json.LastIndexOf("```")].Trim(); }

            var result = JsonSerializer.Deserialize<JdTitleResult>(json, JsonOpts);
            if (result is null) return string.Empty;

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(result.Company) && result.Company != "Unknown") parts.Add(result.Company);
            if (!string.IsNullOrWhiteSpace(result.JobTitle) && result.JobTitle != "Unknown") parts.Add(result.JobTitle);
            if (!string.IsNullOrWhiteSpace(result.Location) && result.Location != "Unknown") parts.Add(result.Location);

            return string.Join(" — ", parts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not extract JD title via AI — using empty title");
            return string.Empty;
        }
    }

    private static JobDescriptionDto MapToDto(JobDescription jd) => new()
    {
        Id = jd.Id,
        SourceType = jd.SourceType,
        Title = jd.Title,
        ParsedContent = jd.ParsedContent,
        CreatedAt = jd.CreatedAt
    };

    private sealed class JdTitleResult
    {
        public string Company { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
