namespace AIJobHelper.Domain.Entities;

public class CoverLetterConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string HeaderTemplate { get; set; } = string.Empty;
    public string FooterTemplate { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CoverLetter> CoverLetters { get; set; } = new List<CoverLetter>();
}
