using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.Enums;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class QuestionState : BaseEntity
{
    private QuestionState()
    {
    }

    internal QuestionState(ConversationSession session, string key, string prompt, int order)
    {
        ArgumentNullException.ThrowIfNull(session);
        Session = session;
        SessionId = session.Id;
        Key = CvProfile.RequiredText(key, nameof(key), 100).ToLowerInvariant();
        Prompt = CvProfile.RequiredText(prompt, nameof(prompt), 2000);
        Order = Math.Max(0, order);
        Status = QuestionStatus.Pending;
    }

    public Guid SessionId { get; private set; }

    [JsonIgnore]
    public ConversationSession? Session { get; private set; }

    public string Key { get; private set; } = string.Empty;
    public string Prompt { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public QuestionStatus Status { get; private set; }
    public string? Answer { get; private set; }
    public DateTimeOffset? AskedAtUtc { get; private set; }
    public DateTimeOffset? AnsweredAtUtc { get; private set; }

    public void MarkAsked(Guid? actorId = null)
    {
        EnsurePendingOrAsked();
        Status = QuestionStatus.Asked;
        AskedAtUtc ??= DateTimeOffset.UtcNow;
        MarkUpdated(actorId);
    }

    public void RecordAnswer(string answer, Guid? actorId = null)
    {
        if (Status is QuestionStatus.Answered or QuestionStatus.Skipped)
        {
            throw new DomainException("This question has already been completed.");
        }

        Answer = CvProfile.RequiredText(answer, nameof(answer), 10_000);
        Status = QuestionStatus.Answered;
        AskedAtUtc ??= DateTimeOffset.UtcNow;
        AnsweredAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated(actorId);
        Session?.OnQuestionCompleted(this);
    }

    public void Skip(Guid? actorId = null)
    {
        if (Status is QuestionStatus.Answered or QuestionStatus.Skipped)
        {
            throw new DomainException("This question has already been completed.");
        }

        Status = QuestionStatus.Skipped;
        AskedAtUtc ??= DateTimeOffset.UtcNow;
        AnsweredAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated(actorId);
        Session?.OnQuestionCompleted(this);
    }

    public void Reset(Guid? actorId = null)
    {
        Status = QuestionStatus.Pending;
        Answer = null;
        AskedAtUtc = null;
        AnsweredAtUtc = null;
        MarkUpdated(actorId);
    }

    private void EnsurePendingOrAsked()
    {
        if (Status is not (QuestionStatus.Pending or QuestionStatus.Asked))
        {
            throw new DomainException("A completed question cannot be asked again.");
        }
    }
}
