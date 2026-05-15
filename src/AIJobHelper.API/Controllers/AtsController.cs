using AIJobHelper.Application.DTOs.Ats;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIJobHelper.API.Controllers;

/// <summary>ATS scoring — compare a resume against a job description.</summary>
[ApiController]
[Route("api/ats")]
[Produces("application/json")]
public class AtsController : ControllerBase
{
    private readonly IAtsService _service;

    public AtsController(IAtsService service) => _service = service;

    /// <summary>Score a resume against a job description and get a detailed ATS analysis report.</summary>
    /// <remarks>
    /// Returns an ATS score (0-100), matched and missing skills, improvement suggestions,
    /// and specific resume edits to clear ATS screening for this role.
    /// Running score again for the same pair creates a new result (previous is kept).
    /// </remarks>
    [HttpPost("score")]
    [ProducesResponseType(typeof(AtsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Score([FromBody] ScoreAtsRequest request, CancellationToken ct)
    {
        var result = await _service.ScoreAsync(request.ResumeId, request.JobDescriptionId, ct);
        return Ok(result);
    }

    /// <summary>Get a stored ATS result by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AtsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List ATS results, optionally filtered by resumeId and/or jobDescriptionId.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AtsResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? resumeId,
        [FromQuery] Guid? jobDescriptionId,
        CancellationToken ct)
    {
        var results = await _service.ListAsync(resumeId, jobDescriptionId, ct);
        return Ok(results);
    }
}
