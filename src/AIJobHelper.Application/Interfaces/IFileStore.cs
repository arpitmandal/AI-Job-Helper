namespace AIJobHelper.Application.Interfaces;

public interface IFileStore
{
    Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> OpenAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
