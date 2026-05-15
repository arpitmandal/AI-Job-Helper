using System.ComponentModel.DataAnnotations;

namespace AIJobHelper.Application.DTOs.CoverLetter;

public class UpdateCoverLetterConfigRequest
{
    [Required, MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Instructions { get; set; } = string.Empty;

    [Required]
    public string HeaderTemplate { get; set; } = string.Empty;

    [Required]
    public string FooterTemplate { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
