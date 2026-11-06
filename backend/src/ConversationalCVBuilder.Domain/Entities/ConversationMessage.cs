using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.Enums;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class ConversationMessage : BaseEntity
{
    private ConversationMessage()
    {
    }

    internal ConversationMessage(
        ConversationSession session,
        ConversationMessageRole role,
        string content,
        ConversationMessageType type,
        int sequence,
        string? audioContentType)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        Session = session;
        SessionId = session.Id;
        Role = role;
        Content = NormalizeContent(content);
        Type = type;
        Sequence = sequence;
        AudioContentType = NormalizeAudioContentType(type, audioContentType);
    }

    public Guid SessionId { get; private set; }

    [JsonIgnore]
    public ConversationSession? Session { get; private set; }

    public ConversationMessageRole Role { get; private set; }
    public ConversationMessageType Type { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int Sequence { get; private set; }
    public string? AudioContentType { get; private set; }

    private static string NormalizeContent(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        var normalized = content.Trim();
        if (normalized.Length > 20_000)
        {
            throw new ArgumentOutOfRangeException(nameof(content), "A conversation message cannot exceed 20,000 characters.");
        }

        return normalized;
    }

    private static string? NormalizeAudioContentType(ConversationMessageType type, string? contentType)
    {
        if (type != ConversationMessageType.VoiceTranscript)
        {
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentException("Audio metadata is only valid for a voice transcript.", nameof(contentType));
            }

            return null;
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        var normalized = contentType.Trim().ToLowerInvariant();
        if (!normalized.StartsWith("audio/", StringComparison.Ordinal))
        {
            throw new ArgumentException("A voice message must use an audio media type.", nameof(contentType));
        }

        return normalized;
    }
}
