namespace AIJobHelper.Application.DTOs.JobDescription;

public class JobDescriptionDto
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ParsedContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
