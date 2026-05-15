namespace AIJobHelper.Application.Interfaces;

public interface IGeminiClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
