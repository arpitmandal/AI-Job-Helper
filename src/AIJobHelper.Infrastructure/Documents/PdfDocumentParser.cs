using AIJobHelper.Application.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AIJobHelper.Infrastructure.Documents;

public class PdfDocumentParser : IDocumentParser
{
    public bool CanParse(string fileExtension) =>
        string.Equals(fileExtension, "pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractTextAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        // PdfPig is synchronous; wrap in Task.Run to avoid blocking async context
        return Task.Run(() =>
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;

            using var pdf = PdfDocument.Open(ms.ToArray());
            var sb = new System.Text.StringBuilder();

            foreach (Page page in pdf.GetPages())
                sb.AppendLine(page.Text);

            return sb.ToString();
        }, cancellationToken);
    }
}
