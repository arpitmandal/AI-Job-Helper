namespace AIJobHelper.Application.Interfaces;

public interface IUrlContentFetcher
{
    /// <summary>Fetches and returns the plain-text content from a URL. Throws InvalidOperationException if the page cannot be scraped.</summary>
    Task<string> FetchAsync(string url, CancellationToken ct = default);
}
