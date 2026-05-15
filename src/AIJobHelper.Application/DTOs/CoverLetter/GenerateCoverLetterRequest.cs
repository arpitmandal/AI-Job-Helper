using System.ComponentModel.DataAnnotations;

namespace AIJobHelper.Application.DTOs.CoverLetter;

public class GenerateCoverLetterRequest : IValidatableObject
{
    /// <summary>ID of the resume to use as the candidate's background.</summary>
    [Required]
    public Guid ResumeId { get; set; }

    /// <summary>How to provide the job description: "stored", "text", or "url".</summary>
    [Required]
    public string JdSourceType { get; set; } = string.Empty;

    /// <summary>Required when JdSourceType is "stored" — the ID of a saved job description.</summary>
    public Guid? JobDescriptionId { get; set; }

    /// <summary>Required when JdSourceType is "text" — paste the job description directly. When JdSourceType is "url" — provide the job posting URL.</summary>
    public string? JdContent { get; set; }

    /// <summary>ID of the cover letter config (instructions + header + footer). Uses the default config if omitted.</summary>
    public Guid? ConfigId { get; set; }

    /// <summary>Output PDF filename without extension (e.g. "john-doe-coverletter"). Defaults to "cover-letter" if omitted.</summary>
    public string? FileName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var type = JdSourceType?.ToLowerInvariant();

        if (type != "stored" && type != "text" && type != "url")
            yield return new ValidationResult(
                "JdSourceType must be one of: 'stored', 'text', 'url'.",
                new[] { nameof(JdSourceType) });

        if (type == "stored" && !JobDescriptionId.HasValue)
            yield return new ValidationResult(
                "JobDescriptionId is required when JdSourceType is 'stored'.",
                new[] { nameof(JobDescriptionId) });

        if ((type == "text" || type == "url") && string.IsNullOrWhiteSpace(JdContent))
            yield return new ValidationResult(
                $"JdContent is required when JdSourceType is '{type}'.",
                new[] { nameof(JdContent) });
    }
}
