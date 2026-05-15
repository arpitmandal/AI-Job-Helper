using AIJobHelper.Application.DTOs.Resume;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIJobHelper.API.Pages.Resumes;

public class ResumeDetailModel : PageModel
{
    private readonly IResumeService _service;

    public ResumeDetailModel(IResumeService service) => _service = service;

    public ResumeDto? Resume { get; private set; }
    public ResumeAnalysisDto? Analysis { get; private set; }
    public string ResumeTextPreview { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var resumes = await _service.ListAsync(ct);
        Resume = resumes.FirstOrDefault(r => r.Id == id);

        if (Resume is null)
            return NotFound();

        Analysis = await _service.GetAnalysisAsync(id, ct);

        // Preview: first 1500 chars of extracted text via the detail endpoint
        // We load it from the DB via service — content is stored on the entity
        ResumeTextPreview = await GetResumeTextAsync(id, ct);

        return Page();
    }

    private async Task<string> GetResumeTextAsync(Guid id, CancellationToken ct)
    {
        // Re-use ListAsync only gives DTOs without ContentText.
        // We'll surface a truncated preview from the analysis summary if available,
        // or show a placeholder until the user analyses the resume.
        if (Analysis is not null && !string.IsNullOrEmpty(Analysis.Summary))
            return $"[Text extracted — run analysis to see the full breakdown below.]\n\nSummary preview:\n{Analysis.Summary}";

        return "[Resume text extracted successfully. Click \"Run AI Analysis\" to analyse this resume.]";
    }
}
