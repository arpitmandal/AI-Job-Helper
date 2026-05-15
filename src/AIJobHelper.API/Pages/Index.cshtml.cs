using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIJobHelper.API.Pages;

public class IndexModel : PageModel
{
    private readonly IResumeService _resumes;
    private readonly IAtsService _ats;
    private readonly ICoverLetterService _coverLetters;

    public IndexModel(IResumeService resumes, IAtsService ats, ICoverLetterService coverLetters)
    {
        _resumes = resumes;
        _ats = ats;
        _coverLetters = coverLetters;
    }

    public int ResumeCount { get; private set; }
    public int AtsCount { get; private set; }
    public int CoverLetterCount { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var resumeList = await _resumes.ListAsync(ct);
        ResumeCount = resumeList.Count();

        var atsList = await _ats.ListAsync(null, null, ct);
        AtsCount = atsList.Count();

        var clList = await _coverLetters.ListAsync(null, ct);
        CoverLetterCount = clList.Count();
    }
}
