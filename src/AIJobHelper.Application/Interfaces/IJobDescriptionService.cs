using AIJobHelper.Application.DTOs.JobDescription;

namespace AIJobHelper.Application.Interfaces;

public interface IJobDescriptionService
{
    Task<JobDescriptionDto> CreateAsync(string content, CancellationToken ct = default);
    Task<JobDescriptionDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<JobDescriptionDto>> ListAsync(CancellationToken ct = default);
}
