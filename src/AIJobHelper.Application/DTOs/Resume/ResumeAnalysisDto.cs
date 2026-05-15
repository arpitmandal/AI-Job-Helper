namespace AIJobHelper.Application.DTOs.Resume;

public class ResumeAnalysisDto
{
    public Guid Id { get; set; }
    public Guid ResumeId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
