using AIJobHelper.Application.DTOs.CoverLetter;
using AIJobHelper.Application.DTOs.JobDescription;
using AIJobHelper.Application.DTOs.Resume;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIJobHelper.API.Pages.CoverLetters;

public class GenerateModel : PageModel
{
    private readonly IResumeService _resumes;
    private readonly IJobDescriptionService _jds;
    private readonly ICoverLetterService _coverLetters;

    public GenerateModel(IResumeService resumes, IJobDescriptionService jds, ICoverLetterService coverLetters)
    {
        _resumes = resumes;
        _jds = jds;
        _coverLetters = coverLetters;
    }

    public IEnumerable<ResumeDto> Resumes { get; private set; } = Enumerable.Empty<ResumeDto>();
    public IEnumerable<JobDescriptionDto> JobDescriptions { get; private set; } = Enumerable.Empty<JobDescriptionDto>();
    public IEnumerable<CoverLetterConfigDto> Configs { get; private set; } = Enumerable.Empty<CoverLetterConfigDto>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Resumes = await _resumes.ListAsync(ct);
        JobDescriptions = await _jds.ListAsync(ct);
        Configs = await _coverLetters.ListConfigsAsync(ct);
    }
}
