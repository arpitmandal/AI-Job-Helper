namespace AIJobHelper.Application.DTOs.Ats;

public class AtsResultDto
{
    public Guid Id { get; set; }
    public Guid ResumeId { get; set; }
    public Guid JobDescriptionId { get; set; }
    public decimal Score { get; set; }
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public List<string> AtsChanges { get; set; } = new();
    public string AiSummary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
