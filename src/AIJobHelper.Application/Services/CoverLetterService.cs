using AIJobHelper.Application.DTOs.CoverLetter;
using AIJobHelper.Application.Interfaces;
using AIJobHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIJobHelper.Application.Services;

public class CoverLetterService : ICoverLetterService
{
    private readonly IApplicationDbContext _db;
    private readonly IGeminiClient _gemini;
    private readonly IUrlContentFetcher _fetcher;
    private readonly IPdfGenerator _pdf;
    private readonly IFileStore _fileStore;
    private readonly ILogger<CoverLetterService> _logger;

    public CoverLetterService(
        IApplicationDbContext db,
        IGeminiClient gemini,
        IUrlContentFetcher fetcher,
        IPdfGenerator pdf,
        IFileStore fileStore,
        ILogger<CoverLetterService> logger)
    {
        _db = db;
        _gemini = gemini;
        _fetcher = fetcher;
        _pdf = pdf;
        _fileStore = fileStore;
        _logger = logger;
    }

    // ── Config management ──────────────────────────────────────────────────

    public async Task<CoverLetterConfigDto> CreateConfigAsync(CreateCoverLetterConfigRequest request, CancellationToken ct = default)
    {
        if (request.IsDefault)
            await ClearDefaultFlagAsync(ct);

        var config = new CoverLetterConfig
        {
            Name = request.Name,
            Instructions = request.Instructions,
            HeaderTemplate = request.HeaderTemplate,
            FooterTemplate = request.FooterTemplate,
            IsDefault = request.IsDefault
        };

        _db.CoverLetterConfigs.Add(config);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("CoverLetterConfig {Id} created", config.Id);
        return MapConfigToDto(config);
    }

    public async Task<CoverLetterConfigDto?> GetConfigAsync(Guid id, CancellationToken ct = default)
    {
        var config = await _db.CoverLetterConfigs.FirstOrDefaultAsync(c => c.Id == id, ct);
        return config is null ? null : MapConfigToDto(config);
    }

    public async Task<IEnumerable<CoverLetterConfigDto>> ListConfigsAsync(CancellationToken ct = default)
    {
        var configs = await _db.CoverLetterConfigs.OrderBy(c => c.Name).ToListAsync(ct);
        return configs.Select(MapConfigToDto);
    }

    public async Task<CoverLetterConfigDto> UpdateConfigAsync(Guid id, UpdateCoverLetterConfigRequest request, CancellationToken ct = default)
    {
        var config = await _db.CoverLetterConfigs.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Config {id} not found.");

        if (request.IsDefault && !config.IsDefault)
            await ClearDefaultFlagAsync(ct);

        config.Name = request.Name;
        config.Instructions = request.Instructions;
        config.HeaderTemplate = request.HeaderTemplate;
        config.FooterTemplate = request.FooterTemplate;
        config.IsDefault = request.IsDefault;
        config.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapConfigToDto(config);
    }

    public async Task DeleteConfigAsync(Guid id, CancellationToken ct = default)
    {
        var config = await _db.CoverLetterConfigs.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Config {id} not found.");

        _db.CoverLetterConfigs.Remove(config);
        await _db.SaveChangesAsync(ct);
    }

    // ── Generation ─────────────────────────────────────────────────────────

    public async Task<CoverLetterDto> GenerateAsync(GenerateCoverLetterRequest request, CancellationToken ct = default)
    {
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == request.ResumeId, ct)
            ?? throw new KeyNotFoundException($"Resume {request.ResumeId} not found.");

        var jdText = await ResolveJdTextAsync(request, ct);

        var config = await ResolveConfigAsync(request.ConfigId, ct);

        _logger.LogInformation("Generating cover letter for resume {ResumeId}", request.ResumeId);

        var prompt = BuildPrompt(config.Instructions, resume.ContentText, jdText);
        var body = await _gemini.GenerateAsync(prompt, ct);

        var fileName = SanitizeFileName(request.FileName) is { Length: > 0 } f ? f : "cover-letter";

        var pdfBytes = _pdf.Generate(config.HeaderTemplate, body, config.FooterTemplate, fileName);
        var pdfFileName = $"{fileName}.pdf";
        var pdfPath = await _fileStore.SaveAsync(pdfFileName, new MemoryStream(pdfBytes), ct);

        var letter = new CoverLetter
        {
            ResumeId = request.ResumeId,
            JobDescriptionId = request.JobDescriptionId,
            ConfigId = config.Id,
            Content = body,
            FileName = fileName,
            PdfPath = pdfPath
        };

        _db.CoverLetters.Add(letter);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Cover letter {Id} generated and PDF saved to {Path}", letter.Id, pdfPath);
        return MapLetterToDto(letter);
    }

    public async Task<CoverLetterDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var letter = await _db.CoverLetters.FirstOrDefaultAsync(l => l.Id == id, ct);
        return letter is null ? null : MapLetterToDto(letter);
    }

    public async Task<IEnumerable<CoverLetterDto>> ListAsync(Guid? resumeId, CancellationToken ct = default)
    {
        var query = _db.CoverLetters.AsQueryable();
        if (resumeId.HasValue)
            query = query.Where(l => l.ResumeId == resumeId.Value);

        var letters = await query.OrderByDescending(l => l.CreatedAt).ToListAsync(ct);
        return letters.Select(MapLetterToDto);
    }

    public async Task<(Stream PdfStream, string FileName)> GetPdfAsync(Guid id, CancellationToken ct = default)
    {
        var letter = await _db.CoverLetters.FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Cover letter {id} not found.");

        if (string.IsNullOrEmpty(letter.PdfPath))
            throw new InvalidOperationException("PDF has not been generated for this cover letter.");

        var stream = await _fileStore.OpenAsync(letter.PdfPath, ct);
        return (stream, $"{letter.FileName}.pdf");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<string> ResolveJdTextAsync(GenerateCoverLetterRequest request, CancellationToken ct)
    {
        var type = request.JdSourceType.ToLowerInvariant();

        if (type == "stored")
        {
            var jd = await _db.JobDescriptions.FirstOrDefaultAsync(j => j.Id == request.JobDescriptionId, ct)
                ?? throw new KeyNotFoundException($"Job description {request.JobDescriptionId} not found.");
            return jd.ParsedContent;
        }

        if (type == "url")
            return await _fetcher.FetchAsync(request.JdContent!, ct);

        return request.JdContent!;
    }

    private async Task<CoverLetterConfig> ResolveConfigAsync(Guid? configId, CancellationToken ct)
    {
        if (configId.HasValue)
        {
            return await _db.CoverLetterConfigs.FirstOrDefaultAsync(c => c.Id == configId.Value, ct)
                ?? throw new KeyNotFoundException($"Cover letter config {configId} not found.");
        }

        var defaultConfig = await _db.CoverLetterConfigs.FirstOrDefaultAsync(c => c.IsDefault, ct);
        return defaultConfig
            ?? throw new InvalidOperationException(
                "No configId was provided and no default config exists. " +
                "Create a config via POST /api/cover-letter-configs and set IsDefault=true, or pass a configId.");
    }

    private async Task ClearDefaultFlagAsync(CancellationToken ct)
    {
        var current = await _db.CoverLetterConfigs.FirstOrDefaultAsync(c => c.IsDefault, ct);
        if (current is not null)
        {
            current.IsDefault = false;
            current.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static string BuildPrompt(string instructions, string resumeText, string jdText) =>
        $"""
        You are an expert career coach and professional writer. Generate the body section of a cover letter.

        Instructions and guidelines to follow:
        {instructions}

        Rules:
        - Write ONLY the body paragraphs — do not include a salutation, header, or closing signature.
        - Connect the candidate's specific experience directly to the requirements in the job description.
        - Be concise, professional, and persuasive.
        - Do not use generic filler phrases.
        - Return only the cover letter body text — no JSON, no markdown, no extra commentary.

        Candidate's Resume:
        {resumeText}

        Job Description:
        {jdText}
        """;

    private static string SanitizeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Where(c => !invalid.Contains(c)).ToArray()).Trim();
    }

    private static CoverLetterConfigDto MapConfigToDto(CoverLetterConfig c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Instructions = c.Instructions,
        HeaderTemplate = c.HeaderTemplate,
        FooterTemplate = c.FooterTemplate,
        IsDefault = c.IsDefault,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    private static CoverLetterDto MapLetterToDto(CoverLetter l) => new()
    {
        Id = l.Id,
        ResumeId = l.ResumeId,
        JobDescriptionId = l.JobDescriptionId,
        ConfigId = l.ConfigId,
        Content = l.Content,
        FileName = l.FileName,
        CreatedAt = l.CreatedAt
    };
}
