namespace AIJobHelper.Domain.Entities;

public class ResumeAnalysis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ResumeId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Strengths { get; set; } = string.Empty;   // JSON array
    public string Weaknesses { get; set; } = string.Empty;  // JSON array
    public string Suggestions { get; set; } = string.Empty; // JSON array of job titles
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Resume Resume { get; set; } = null!;
}
