using AIJobHelper.Application.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AIJobHelper.Infrastructure.Web;

public class UrlContentFetcher : IUrlContentFetcher
{
    private readonly HttpClient _http;
    private readonly ILogger<UrlContentFetcher> _logger;

    public UrlContentFetcher(HttpClient http, ILogger<UrlContentFetcher> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<string> FetchAsync(string url, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching content from {Url}", url);

        HttpResponseMessage response;
        try
        {
            response = await _http.GetAsync(url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Network error fetching {Url}", url);
            throw new InvalidOperationException($"Could not reach the URL: {ex.Message}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("URL {Url} returned {Status}", url, (int)response.StatusCode);
            throw new InvalidOperationException(
                $"The job posting URL returned HTTP {(int)response.StatusCode}. " +
                "This site may block automated access. Please paste the job description text directly instead.");
        }

        var html = await response.Content.ReadAsStringAsync(ct);

        var text = ExtractText(html);

        if (string.IsNullOrWhiteSpace(text) || text.Length < 100)
        {
            _logger.LogWarning("Extracted text too short from {Url} ({Length} chars)", url, text.Length);
            throw new InvalidOperationException(
                "Could not extract meaningful content from this URL. " +
                "The site may require JavaScript or block scrapers. Please paste the job description text directly instead.");
        }

        _logger.LogInformation("Extracted {Length} chars from {Url}", text.Length, url);
        return text;
    }

    private static string ExtractText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Remove script, style, nav, header, footer — leaves body content
        var nodesToRemove = doc.DocumentNode
            .SelectNodes("//script|//style|//nav|//header|//footer|//aside|//noscript")
            ?? Enumerable.Empty<HtmlNode>();

        foreach (var node in nodesToRemove.ToList())
            node.Remove();

        var text = doc.DocumentNode.InnerText;

        // Collapse whitespace
        text = Regex.Replace(text, @"\s+", " ").Trim();

        return text;
    }
}
