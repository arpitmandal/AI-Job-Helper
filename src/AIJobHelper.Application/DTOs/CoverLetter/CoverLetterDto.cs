namespace AIJobHelper.Application.DTOs.CoverLetter;

public class CoverLetterDto
{
    public Guid Id { get; set; }
    public Guid ResumeId { get; set; }
    public Guid? JobDescriptionId { get; set; }
    public Guid? ConfigId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
