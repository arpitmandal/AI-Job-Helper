namespace AIJobHelper.Domain.Entities;

public class CoverLetter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ResumeId { get; set; }
    public Guid? JobDescriptionId { get; set; }
    public Guid? ConfigId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string PdfPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Resume Resume { get; set; } = null!;
    public JobDescription? JobDescription { get; set; }
    public CoverLetterConfig? Config { get; set; }
}
