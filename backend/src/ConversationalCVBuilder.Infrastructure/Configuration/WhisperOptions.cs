namespace ConversationalCVBuilder.Infrastructure.Configuration;

public sealed class WhisperOptions
{
    public const string SectionName = "Whisper";

    public string BaseUrl { get; set; } = "http://localhost:9000/";
    public string TranscriptionsPath { get; set; } = "v1/audio/transcriptions";
    public string Model { get; set; } = "whisper-1";
    public string? ApiKey { get; set; }
    public string? Language { get; set; }
    public int TimeoutSeconds { get; set; } = 120;
    public long MaximumAudioBytes { get; set; } = 25 * 1024 * 1024;
}
