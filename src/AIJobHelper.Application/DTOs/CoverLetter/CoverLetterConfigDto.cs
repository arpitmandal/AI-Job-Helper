namespace AIJobHelper.Application.DTOs.CoverLetter;

public class CoverLetterConfigDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string HeaderTemplate { get; set; } = string.Empty;
    public string FooterTemplate { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
