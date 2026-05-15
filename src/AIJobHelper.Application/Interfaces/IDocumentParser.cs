namespace AIJobHelper.Application.Interfaces;

public interface IDocumentParser
{
    bool CanParse(string fileExtension);
    Task<string> ExtractTextAsync(Stream stream, CancellationToken cancellationToken = default);
}
