using System.Text;
using AIJobHelper.Application.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AIJobHelper.Infrastructure.Documents;

public class DocxDocumentParser : IDocumentParser
{
    public bool CanParse(string fileExtension) =>
        string.Equals(fileExtension, "docx", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractTextAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var doc = WordprocessingDocument.Open(stream, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body is null) return string.Empty;

            var sb = new StringBuilder();
            foreach (var para in body.Elements<Paragraph>())
            {
                sb.AppendLine(para.InnerText);
            }

            return sb.ToString();
        }, cancellationToken);
    }
}
