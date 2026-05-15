using AIJobHelper.Application.DTOs.JobDescription;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIJobHelper.API.Controllers;

/// <summary>Manage job descriptions used for ATS scoring.</summary>
[ApiController]
[Route("api/job-descriptions")]
[Produces("application/json")]
public class JobDescriptionsController : ControllerBase
{
    private readonly IJobDescriptionService _service;

    public JobDescriptionsController(IJobDescriptionService service) => _service = service;

    /// <summary>Save a job description. Provide plain text or a URL to a job posting.</summary>
    /// <remarks>
    /// If the content starts with http:// or https://, the URL will be fetched and its text extracted.
    /// Otherwise the content is saved as-is.
    /// Returns 422 if a URL is provided but cannot be scraped — in that case paste the text directly.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(JobDescriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateJobDescriptionRequest request, CancellationToken ct)
    {
        var isUrl = request.Content.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    request.Content.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

        try
        {
            var result = await _service.CreateAsync(request.Content, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) when (isUrl)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "URL scraping failed",
                Detail = ex.Message,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
    }

    /// <summary>Get a job description by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobDescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List all saved job descriptions.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JobDescriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var results = await _service.ListAsync(ct);
        return Ok(results);
    }

    /// <summary>Delete a job description and its associated ATS results.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
