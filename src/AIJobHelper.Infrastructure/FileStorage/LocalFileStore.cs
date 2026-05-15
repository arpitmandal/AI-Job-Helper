using AIJobHelper.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIJobHelper.Infrastructure.FileStorage;

public class LocalFileStore : IFileStore
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStore> _logger;

    public LocalFileStore(ILogger<LocalFileStore> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(AppContext.BaseDirectory, "uploads");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var safeName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_basePath, safeName);

        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs, cancellationToken);

        _logger.LogDebug("File saved to {Path}", fullPath);
        return fullPath;
    }

    public Task<Stream> OpenAsync(string filePath, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream>(File.OpenRead(filePath));

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }
}
