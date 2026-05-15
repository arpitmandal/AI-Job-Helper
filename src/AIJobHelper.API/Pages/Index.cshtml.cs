using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIJobHelper.API.Pages;

public class IndexModel : PageModel
{
    private readonly IResumeService _resumes;

    public IndexModel(IResumeService resumes) => _resumes = resumes;

    public int ResumeCount { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var list = await _resumes.ListAsync(ct);
        ResumeCount = list.Count();
    }
}
