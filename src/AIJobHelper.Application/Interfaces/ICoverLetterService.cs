using AIJobHelper.Application.DTOs.CoverLetter;

namespace AIJobHelper.Application.Interfaces;

public interface ICoverLetterService
{
    // Config management
    Task<CoverLetterConfigDto> CreateConfigAsync(CreateCoverLetterConfigRequest request, CancellationToken ct = default);
    Task<CoverLetterConfigDto?> GetConfigAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<CoverLetterConfigDto>> ListConfigsAsync(CancellationToken ct = default);
    Task<CoverLetterConfigDto> UpdateConfigAsync(Guid id, UpdateCoverLetterConfigRequest request, CancellationToken ct = default);
    Task DeleteConfigAsync(Guid id, CancellationToken ct = default);

    // Cover letter generation
    Task<CoverLetterDto> GenerateAsync(GenerateCoverLetterRequest request, CancellationToken ct = default);
    Task<CoverLetterDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<CoverLetterDto>> ListAsync(Guid? resumeId, CancellationToken ct = default);

    /// <summary>Returns the PDF stream and filename for download.</summary>
    Task<(Stream PdfStream, string FileName)> GetPdfAsync(Guid id, CancellationToken ct = default);
}
