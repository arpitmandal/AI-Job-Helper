using AIJobHelper.Application.DTOs.Resume;

namespace AIJobHelper.Application.Interfaces;

public interface IResumeService
{
    Task<ResumeDto> UploadAsync(string fileName, string fileType, Stream fileStream, CancellationToken cancellationToken = default);
    Task<ResumeAnalysisDto> AnalyzeAsync(Guid resumeId, CancellationToken cancellationToken = default);
    Task<ResumeAnalysisDto?> GetAnalysisAsync(Guid resumeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResumeDto>> ListAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid resumeId, CancellationToken cancellationToken = default);
}
