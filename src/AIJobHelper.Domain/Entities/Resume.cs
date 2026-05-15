namespace AIJobHelper.Domain.Entities;

public class Resume
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // pdf | docx
    public string ContentText { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public ResumeAnalysis? Analysis { get; set; }
    public ICollection<AtsResult> AtsResults { get; set; } = new List<AtsResult>();
    public ICollection<CoverLetter> CoverLetters { get; set; } = new List<CoverLetter>();
}
