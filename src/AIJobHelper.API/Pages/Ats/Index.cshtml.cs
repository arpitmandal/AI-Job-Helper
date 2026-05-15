using AIJobHelper.Application.DTOs.Resume;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIJobHelper.API.Pages.Ats;

public class AtsIndexModel : PageModel
{
    private readonly IResumeService _resumes;

    public AtsIndexModel(IResumeService resumes) => _resumes = resumes;

    public IEnumerable<ResumeDto> Resumes { get; private set; } = Enumerable.Empty<ResumeDto>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Resumes = await _resumes.ListAsync(ct);
    }
}
