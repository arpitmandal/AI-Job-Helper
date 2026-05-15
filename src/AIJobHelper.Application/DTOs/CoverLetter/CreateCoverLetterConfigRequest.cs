using System.ComponentModel.DataAnnotations;

namespace AIJobHelper.Application.DTOs.CoverLetter;

public class CreateCoverLetterConfigRequest
{
    /// <summary>A descriptive name for this config (e.g. "Software Engineer - Formal").</summary>
    [Required, MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Guidelines for the AI on tone, length, focus areas, and style.</summary>
    [Required]
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Header section of the cover letter (e.g. name, contact info, date). Plain text or markdown.</summary>
    [Required]
    public string HeaderTemplate { get; set; } = string.Empty;

    /// <summary>Footer/closing section (e.g. "Sincerely, John Doe"). Plain text or markdown.</summary>
    [Required]
    public string FooterTemplate { get; set; } = string.Empty;

    /// <summary>Set this config as the default used when no configId is specified at generation time.</summary>
    public bool IsDefault { get; set; }
}
