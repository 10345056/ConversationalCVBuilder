using System.Net.Http.Json;
using System.Text.Json;
using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ConversationalCVBuilder.Infrastructure.Services;

public sealed class OllamaService(
    HttpClient httpClient,
    IOptions<OllamaOptions> options) : IOllamaService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly OllamaOptions _options = options.Value;

    public async Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        using var response = await httpClient.PostAsJsonAsync(
            "api/generate",
            new GenerateRequest(
                _options.Model,
                prompt,
                Stream: false,
                new GenerateOptions(_options.Temperature)),
            JsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Ollama returned {(int)response.StatusCode} ({response.StatusCode}): {Truncate(responseBody)}",
                inner: null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<GenerateResponse>(
            JsonOptions,
            cancellationToken);
        if (result is null || string.IsNullOrWhiteSpace(result.Response))
        {
            throw new InvalidDataException("Ollama returned an empty or invalid generation response.");
        }

        return result.Response.Trim();
    }

    private static string Truncate(string value) =>
        value.Length <= 1000 ? value : value[..1000];

    private sealed record GenerateRequest(
        string Model,
        string Prompt,
        bool Stream,
        GenerateOptions Options);

    private sealed record GenerateOptions(double Temperature);
    private sealed record GenerateResponse(string? Response);
}
