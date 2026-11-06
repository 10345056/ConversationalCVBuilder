using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ConversationalCVBuilder.Infrastructure.Services;

public sealed class WhisperService(
    HttpClient httpClient,
    IOptions<WhisperOptions> options) : IWhisperService
{
    private static readonly HashSet<string> SupportedAudioExtensions = new(
        [".flac", ".m4a", ".mp3", ".mp4", ".mpeg", ".mpga", ".ogg", ".wav", ".webm"],
        StringComparer.OrdinalIgnoreCase);
    private readonly WhisperOptions _options = options.Value;

    public async Task<string> TranscribeAsync(
        Stream audio,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audio);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (!audio.CanRead)
        {
            throw new ArgumentException("The audio stream must be readable.", nameof(audio));
        }

        if (!SupportedAudioExtensions.Contains(Path.GetExtension(Path.GetFileName(fileName))))
        {
            throw new InvalidDataException("The audio file format is not supported.");
        }

        await using var boundedAudio = await CopyAudioAsync(audio, cancellationToken);
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(boundedAudio);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        form.Add(fileContent, "file", Path.GetFileName(fileName));
        form.Add(new StringContent(_options.Model), "model");
        if (!string.IsNullOrWhiteSpace(_options.Language))
        {
            form.Add(new StringContent(_options.Language), "language");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TranscriptionsPath)
        {
            Content = form
        };
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Whisper returned {(int)response.StatusCode} ({response.StatusCode}): {Truncate(responseBody)}",
                inner: null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<TranscriptionResponse>(
            cancellationToken: cancellationToken);
        if (result is null || string.IsNullOrWhiteSpace(result.Text))
        {
            throw new InvalidDataException("Whisper returned an empty or invalid transcription response.");
        }

        return result.Text.Trim();
    }

    private static string Truncate(string value) =>
        value.Length <= 1000 ? value : value[..1000];

    private async Task<MemoryStream> CopyAudioAsync(
        Stream audio,
        CancellationToken cancellationToken)
    {
        var boundedAudio = new MemoryStream();
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await audio.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (boundedAudio.Length + bytesRead > _options.MaximumAudioBytes)
            {
                await boundedAudio.DisposeAsync();
                throw new ArgumentOutOfRangeException(nameof(audio), "The audio file exceeds the configured size limit.");
            }

            await boundedAudio.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        if (boundedAudio.Length == 0)
        {
            await boundedAudio.DisposeAsync();
            throw new InvalidDataException("The audio file is empty.");
        }

        boundedAudio.Position = 0;
        return boundedAudio;
    }

    private sealed record TranscriptionResponse(string? Text);
}
