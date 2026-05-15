using System.ComponentModel.DataAnnotations;

namespace AIJobHelper.Application.DTOs.JobDescription;

public class CreateJobDescriptionRequest
{
    /// <summary>Paste the job description text directly, or provide a URL to the job posting (http/https).</summary>
    [Required, MinLength(10)]
    public string Content { get; set; } = string.Empty;
}
