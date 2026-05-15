using System.Net.Http.Json;
using System.Text.Json;
using AIJobHelper.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIJobHelper.Infrastructure.AI;

public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(HttpClient http, IOptions<GeminiOptions> options, ILogger<GeminiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var url = $"/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        _logger.LogDebug("Calling Gemini model {Model}", _options.Model);

        var response = await _http.PostAsJsonAsync(url, body, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(json, cancellationToken: cancellationToken);

        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? throw new InvalidOperationException("Gemini returned an empty response.");
    }
}
