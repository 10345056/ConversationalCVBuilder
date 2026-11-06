namespace ConversationalCVBuilder.Application.Abstractions;

public interface IWhisperService
{
    Task<string> TranscribeAsync(
        Stream audio,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default);
}
