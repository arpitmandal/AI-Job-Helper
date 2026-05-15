using AIJobHelper.Application.DTOs.CoverLetter;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIJobHelper.API.Controllers;

/// <summary>Manage cover letter configuration templates (instructions, header, footer).</summary>
[ApiController]
[Route("api/cover-letter-configs")]
[Produces("application/json")]
public class CoverLetterConfigsController : ControllerBase
{
    private readonly ICoverLetterService _service;

    public CoverLetterConfigsController(ICoverLetterService service) => _service = service;

    /// <summary>Create a new cover letter config with instructions, header, and footer templates.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CoverLetterConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCoverLetterConfigRequest request, CancellationToken ct)
    {
        var result = await _service.CreateConfigAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Get a cover letter config by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CoverLetterConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetConfigAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List all cover letter configs.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CoverLetterConfigDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var results = await _service.ListConfigsAsync(ct);
        return Ok(results);
    }

    /// <summary>Update a cover letter config.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CoverLetterConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCoverLetterConfigRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateConfigAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Delete a cover letter config.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteConfigAsync(id, ct);
        return NoContent();
    }
}
