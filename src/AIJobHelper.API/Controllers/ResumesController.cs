using AIJobHelper.Application.DTOs.Resume;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIJobHelper.API.Controllers;

[ApiController]
[Route("api/resumes")]
[Produces("application/json")]
public class ResumesController : ControllerBase
{
    private static readonly string[] AllowedExtensions = ["pdf", "docx"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IResumeService _resumeService;
    private readonly ILogger<ResumesController> _logger;

    public ResumesController(IResumeService resumeService, ILogger<ResumesController> logger)
    {
        _resumeService = resumeService;
        _logger = logger;
    }

    /// <summary>Upload a resume (PDF or DOCX). Returns the created resume ID.</summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ResumeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(Problem("No file provided.", statusCode: 400));

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(Problem("File exceeds 10 MB limit.", statusCode: 400));

        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(Problem($"Unsupported file type '{ext}'. Allowed: pdf, docx.", statusCode: 400));

        await using var stream = file.OpenReadStream();
        var result = await _resumeService.UploadAsync(file.FileName, ext, stream, cancellationToken);

        _logger.LogInformation("Resume {FileName} uploaded as {ResumeId}", file.FileName, result.Id);
        return CreatedAtAction(nameof(GetAnalysis), new { id = result.Id }, result);
    }

    /// <summary>Run AI analysis on an uploaded resume. Re-running overwrites the previous analysis.</summary>
    [HttpPost("{id:guid}/analyze")]
    [ProducesResponseType(typeof(ResumeAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Analyze(Guid id, CancellationToken cancellationToken)
    {
        var result = await _resumeService.AnalyzeAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Retrieve the stored AI analysis for a resume.</summary>
    [HttpGet("{id:guid}/analysis")]
    [ProducesResponseType(typeof(ResumeAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnalysis(Guid id, CancellationToken cancellationToken)
    {
        var result = await _resumeService.GetAnalysisAsync(id, cancellationToken);
        if (result is null)
            return NotFound(Problem($"No analysis found for resume {id}. Run POST /api/resumes/{id}/analyze first.", statusCode: 404));

        return Ok(result);
    }

    /// <summary>List all uploaded resumes ordered by upload date (newest first).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ResumeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _resumeService.ListAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a resume and its associated analysis.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _resumeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
