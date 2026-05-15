using AIJobHelper.Application.DTOs.Resume;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIJobHelper.API.Pages.Resumes;

public class ResumesIndexModel : PageModel
{
    private readonly IResumeService _service;
    private readonly ILogger<ResumesIndexModel> _logger;

    public ResumesIndexModel(IResumeService service, ILogger<ResumesIndexModel> logger)
    {
        _service = service;
        _logger = logger;
    }

    public IEnumerable<ResumeDto> Resumes { get; private set; } = Enumerable.Empty<ResumeDto>();
    public string? SuccessMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Resumes = await _service.ListAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            ErrorMessage = "Please select a file.";
            Resumes = await _service.ListAsync(ct);
            return Page();
        }

        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (ext != "pdf" && ext != "docx")
        {
            ErrorMessage = "Only PDF and DOCX files are supported.";
            Resumes = await _service.ListAsync(ct);
            return Page();
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            ErrorMessage = "File size must be 10 MB or less.";
            Resumes = await _service.ListAsync(ct);
            return Page();
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _service.UploadAsync(file.FileName, ext, stream, ct);
            _logger.LogInformation("Resume uploaded via UI: {ResumeId}", result.Id);
            return RedirectToPage("/Resumes/Detail", new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed");
            ErrorMessage = $"Upload failed: {ex.Message}";
            Resumes = await _service.ListAsync(ct);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, ct);
            SuccessMessage = "Resume deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed for resume {Id}", id);
            ErrorMessage = $"Could not delete resume: {ex.Message}";
        }

        Resumes = await _service.ListAsync(ct);
        return Page();
    }
}
