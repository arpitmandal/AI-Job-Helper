using System.ComponentModel.DataAnnotations;

namespace AIJobHelper.Application.DTOs.Ats;

public class ScoreAtsRequest
{
    [Required]
    public Guid ResumeId { get; set; }

    [Required]
    public Guid JobDescriptionId { get; set; }
}
