using System.Text.Json;
using AIJobHelper.Application.DTOs.Resume;
using AIJobHelper.Application.Interfaces;
using AIJobHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIJobHelper.Application.Services;

public class ResumeService : IResumeService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IApplicationDbContext _db;
    private readonly IEnumerable<IDocumentParser> _parsers;
    private readonly IGeminiClient _gemini;
    private readonly ILogger<ResumeService> _logger;

    public ResumeService(
        IApplicationDbContext db,
        IEnumerable<IDocumentParser> parsers,
        IGeminiClient gemini,
        ILogger<ResumeService> logger)
    {
        _db = db;
        _parsers = parsers;
        _gemini = gemini;
        _logger = logger;
    }

    public async Task<ResumeDto> UploadAsync(string fileName, string fileType, Stream fileStream, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        var parser = _parsers.FirstOrDefault(p => p.CanParse(ext))
            ?? throw new InvalidOperationException($"Unsupported file type: {ext}");

        _logger.LogInformation("Parsing resume file {FileName}", fileName);
        var text = await parser.ExtractTextAsync(fileStream, cancellationToken);

        var resume = new Resume
        {
            FileName = fileName,
            FileType = ext,
            ContentText = text
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Resume {ResumeId} saved", resume.Id);
        return MapToDto(resume);
    }

    public async Task<ResumeAnalysisDto> AnalyzeAsync(Guid resumeId, CancellationToken cancellationToken = default)
    {
        var resume = await _db.Resumes
            .Include(r => r.Analysis)
            .FirstOrDefaultAsync(r => r.Id == resumeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Resume {resumeId} not found");

        _logger.LogInformation("Running Gemini analysis for resume {ResumeId}", resumeId);

        var prompt = BuildAnalysisPrompt(resume.ContentText);
        var raw = await _gemini.GenerateAsync(prompt, cancellationToken);

        var parsed = ParseAnalysisResponse(raw);

        if (resume.Analysis is not null)
            _db.ResumeAnalyses.Remove(resume.Analysis);

        var analysis = new ResumeAnalysis
        {
            ResumeId = resumeId,
            Summary = parsed.Summary,
            Strengths = JsonSerializer.Serialize(parsed.Strengths),
            Weaknesses = JsonSerializer.Serialize(parsed.Weaknesses),
            Suggestions = JsonSerializer.Serialize(parsed.Suggestions)
        };

        _db.ResumeAnalyses.Add(analysis);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Analysis {AnalysisId} stored for resume {ResumeId}", analysis.Id, resumeId);
        return MapToDto(analysis);
    }

    public async Task<ResumeAnalysisDto?> GetAnalysisAsync(Guid resumeId, CancellationToken cancellationToken = default)
    {
        var analysis = await _db.ResumeAnalyses
            .FirstOrDefaultAsync(a => a.ResumeId == resumeId, cancellationToken);

        return analysis is null ? null : MapToDto(analysis);
    }

    public async Task<IEnumerable<ResumeDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var resumes = await _db.Resumes
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync(cancellationToken);

        return resumes.Select(MapToDto);
    }

    public async Task DeleteAsync(Guid resumeId, CancellationToken cancellationToken = default)
    {
        var resume = await _db.Resumes
            .Include(r => r.Analysis)
            .FirstOrDefaultAsync(r => r.Id == resumeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Resume {resumeId} not found");

        _db.Resumes.Remove(resume);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Resume {ResumeId} deleted", resumeId);
    }

    private static string BuildAnalysisPrompt(string resumeText) =>
        $"""
        Analyze the following resume and respond with ONLY valid JSON — no markdown, no code blocks, no extra text.
        The JSON must have exactly these keys:
        - "summary": string — a concise professional overview (2-3 sentences)
        - "strengths": array of strings — key strengths and qualifications
        - "weaknesses": array of strings — areas for improvement
        - "suggestions": array of strings — recommended job titles that match the profile

        Resume:
        {resumeText}
        """;

    private GeminiAnalysisResult ParseAnalysisResponse(string raw)
    {
        try
        {
            // Strip potential markdown code fences if Gemini wraps in ```json
            var json = raw.Trim();
            if (json.StartsWith("```"))
            {
                json = json[(json.IndexOf('\n') + 1)..];
                json = json[..json.LastIndexOf("```")].Trim();
            }

            return JsonSerializer.Deserialize<GeminiAnalysisResult>(json, JsonOpts)
                ?? throw new JsonException("Null deserialization result");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response: {Raw}", raw);
            throw new InvalidOperationException("AI returned an unexpected response format. Please retry.", ex);
        }
    }

    private static ResumeDto MapToDto(Resume r) => new()
    {
        Id = r.Id,
        FileName = r.FileName,
        FileType = r.FileType,
        UploadedAt = r.UploadedAt
    };

    private static ResumeAnalysisDto MapToDto(ResumeAnalysis a) => new()
    {
        Id = a.Id,
        ResumeId = a.ResumeId,
        Summary = a.Summary,
        Strengths = Deserialize(a.Strengths),
        Weaknesses = Deserialize(a.Weaknesses),
        Suggestions = Deserialize(a.Suggestions),
        CreatedAt = a.CreatedAt
    };

    private static List<string> Deserialize(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
        catch { return new(); }
    }

    private sealed class GeminiAnalysisResult
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }
}
