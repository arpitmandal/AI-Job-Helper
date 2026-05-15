namespace AIJobHelper.Domain.Entities;

public class AtsResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ResumeId { get; set; }
    public Guid JobDescriptionId { get; set; }
    public decimal Score { get; set; }
    public string MatchedSkills { get; set; } = string.Empty;  // JSON array
    public string MissingSkills { get; set; } = string.Empty;  // JSON array
    public string Suggestions { get; set; } = string.Empty;    // JSON array
    public string AtsChanges { get; set; } = string.Empty;     // JSON array — specific resume edits to clear ATS
    public string AiSummary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Resume Resume { get; set; } = null!;
    public JobDescription JobDescription { get; set; } = null!;
}
