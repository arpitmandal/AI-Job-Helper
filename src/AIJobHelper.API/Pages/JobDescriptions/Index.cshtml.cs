using AIJobHelper.Application.DTOs.JobDescription;
using AIJobHelper.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIJobHelper.API.Pages.JobDescriptions;

public class IndexModel : PageModel
{
    private readonly IJobDescriptionService _service;

    public IndexModel(IJobDescriptionService service) => _service = service;

    public IEnumerable<JobDescriptionDto> JobDescriptions { get; private set; } = Enumerable.Empty<JobDescriptionDto>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        JobDescriptions = await _service.ListAsync(ct);
    }
}
