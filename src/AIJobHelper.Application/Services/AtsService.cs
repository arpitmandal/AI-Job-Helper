using System.Text.Json;
using AIJobHelper.Application.DTOs.Ats;
using AIJobHelper.Application.Interfaces;
using AIJobHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIJobHelper.Application.Services;

public class AtsService : IAtsService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IApplicationDbContext _db;
    private readonly IGeminiClient _gemini;
    private readonly ILogger<AtsService> _logger;

    public AtsService(IApplicationDbContext db, IGeminiClient gemini, ILogger<AtsService> logger)
    {
        _db = db;
        _gemini = gemini;
        _logger = logger;
    }

    public async Task<AtsResultDto> ScoreAsync(Guid resumeId, Guid jobDescriptionId, CancellationToken ct = default)
    {
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId, ct)
            ?? throw new KeyNotFoundException($"Resume {resumeId} not found.");

        var jd = await _db.JobDescriptions.FirstOrDefaultAsync(j => j.Id == jobDescriptionId, ct)
            ?? throw new KeyNotFoundException($"Job description {jobDescriptionId} not found.");

        _logger.LogInformation("Running ATS scoring for resume {ResumeId} against JD {JdId}", resumeId, jobDescriptionId);

        var prompt = BuildScoringPrompt(resume.ContentText, jd.ParsedContent);
        var raw = await _gemini.GenerateAsync(prompt, ct);
        var parsed = ParseScoringResponse(raw);

        var result = new AtsResult
        {
            ResumeId = resumeId,
            JobDescriptionId = jobDescriptionId,
            Score = parsed.Score,
            MatchedSkills = JsonSerializer.Serialize(parsed.MatchedSkills),
            MissingSkills = JsonSerializer.Serialize(parsed.MissingSkills),
            Suggestions = JsonSerializer.Serialize(parsed.Suggestions),
            AtsChanges = JsonSerializer.Serialize(parsed.AtsChanges),
            AiSummary = parsed.Summary
        };

        _db.AtsResults.Add(result);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("ATS result {ResultId} stored (score: {Score})", result.Id, result.Score);
        return MapToDto(result);
    }

    public async Task<AtsResultDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var result = await _db.AtsResults.FirstOrDefaultAsync(r => r.Id == id, ct);
        return result is null ? null : MapToDto(result);
    }

    public async Task<IEnumerable<AtsResultDto>> ListAsync(Guid? resumeId, Guid? jobDescriptionId, CancellationToken ct = default)
    {
        var query = _db.AtsResults.AsQueryable();

        if (resumeId.HasValue)
            query = query.Where(r => r.ResumeId == resumeId.Value);

        if (jobDescriptionId.HasValue)
            query = query.Where(r => r.JobDescriptionId == jobDescriptionId.Value);

        var results = await query.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        return results.Select(MapToDto);
    }

    private static string BuildScoringPrompt(string resumeText, string jdText) =>
        $"""
        You are an expert ATS (Applicant Tracking System) analyst. Compare the resume to the job description below.
        Respond with ONLY valid JSON — no markdown, no code blocks, no extra text.
        The JSON must have exactly these keys:
        - "score": integer 0-100 — overall ATS compatibility score
        - "matchedSkills": array of strings — skills and keywords from the JD that are present in the resume
        - "missingSkills": array of strings — important skills and keywords from the JD that are absent from the resume
        - "suggestions": array of strings — general recommendations to improve the resume for this role
        - "atsChanges": array of strings — specific actionable edits to make to the resume to clear ATS screening (e.g. "Add 'Kubernetes' to your skills section", "Quantify the DevOps achievement at Company X with metrics")
        - "summary": string — 2-3 sentence overall assessment of the resume-JD fit

        Resume:
        {resumeText}

        Job Description:
        {jdText}
        """;

    private GeminiScoringResult ParseScoringResponse(string raw)
    {
        try
        {
            var json = raw.Trim();
            if (json.StartsWith("```"))
            {
                json = json[(json.IndexOf('\n') + 1)..];
                json = json[..json.LastIndexOf("```")].Trim();
            }

            return JsonSerializer.Deserialize<GeminiScoringResult>(json, JsonOpts)
                ?? throw new JsonException("Null deserialization result");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini ATS response: {Raw}", raw);
            throw new InvalidOperationException("AI returned an unexpected response format. Please retry.", ex);
        }
    }

    private static AtsResultDto MapToDto(AtsResult r) => new()
    {
        Id = r.Id,
        ResumeId = r.ResumeId,
        JobDescriptionId = r.JobDescriptionId,
        Score = r.Score,
        MatchedSkills = Deserialize(r.MatchedSkills),
        MissingSkills = Deserialize(r.MissingSkills),
        Suggestions = Deserialize(r.Suggestions),
        AtsChanges = Deserialize(r.AtsChanges),
        AiSummary = r.AiSummary,
        CreatedAt = r.CreatedAt
    };

    private static List<string> Deserialize(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
        catch { return new(); }
    }

    private sealed class GeminiScoringResult
    {
        public decimal Score { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public List<string> AtsChanges { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}
