using AIJobHelper.Application.DTOs.Ats;

namespace AIJobHelper.Application.Interfaces;

public interface IAtsService
{
    Task<AtsResultDto> ScoreAsync(Guid resumeId, Guid jobDescriptionId, CancellationToken ct = default);
    Task<AtsResultDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<AtsResultDto>> ListAsync(Guid? resumeId, Guid? jobDescriptionId, CancellationToken ct = default);
}
