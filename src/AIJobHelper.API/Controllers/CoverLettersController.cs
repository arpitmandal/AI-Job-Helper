using AIJobHelper.Application.DTOs.CoverLetter;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIJobHelper.API.Controllers;

/// <summary>Generate and retrieve cover letters.</summary>
[ApiController]
[Route("api/cover-letters")]
[Produces("application/json")]
public class CoverLettersController : ControllerBase
{
    private readonly ICoverLetterService _service;

    public CoverLettersController(ICoverLetterService service) => _service = service;

    /// <summary>Generate a cover letter as a PDF.</summary>
    /// <remarks>
    /// Provide a resume (by ID), a job description (from DB, raw text, or URL), and a config.
    /// If no configId is specified the default config is used.
    ///
    /// JdSourceType values:
    /// - "stored" — provide an existing jobDescriptionId from the database
    /// - "text"   — paste the job description text directly into jdContent
    /// - "url"    — provide a job posting URL in jdContent (best-effort scraping)
    ///
    /// After generation, download the PDF via GET /api/cover-letters/{id}/pdf.
    /// </remarks>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(CoverLetterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Generate([FromBody] GenerateCoverLetterRequest request, CancellationToken ct)
    {
        var isUrlSource = request.JdSourceType?.Equals("url", StringComparison.OrdinalIgnoreCase) == true;

        try
        {
            var result = await _service.GenerateAsync(request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (isUrlSource && ex.Message.Contains("URL"))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "URL scraping failed",
                Detail = ex.Message,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
    }

    /// <summary>Get a generated cover letter record by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CoverLetterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List cover letters, optionally filtered by resumeId.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CoverLetterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid? resumeId, CancellationToken ct)
    {
        var results = await _service.ListAsync(resumeId, ct);
        return Ok(results);
    }

    /// <summary>Download the generated cover letter as a PDF file.</summary>
    /// <remarks>Returns the PDF with Content-Disposition: attachment so it downloads automatically.</remarks>
    [HttpGet("{id:guid}/pdf")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken ct)
    {
        var (stream, fileName) = await _service.GetPdfAsync(id, ct);
        return File(stream, "application/pdf", fileName);
    }
}
