namespace AIJobHelper.Domain.Entities;

public class JobDescription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SourceType { get; set; } = string.Empty; // text | url
    public string Title { get; set; } = string.Empty;      // AI-extracted: "Company — Job Title — Location"
    public string RawContent { get; set; } = string.Empty;
    public string ParsedContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AtsResult> AtsResults { get; set; } = new List<AtsResult>();
    public ICollection<CoverLetter> CoverLetters { get; set; } = new List<CoverLetter>();
}
