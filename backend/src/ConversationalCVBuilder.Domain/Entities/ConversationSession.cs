using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.Enums;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class ConversationSession : AggregateRoot
{
    private readonly List<ConversationMessage> _messages = [];
    private readonly List<QuestionState> _questionStates = [];

    private ConversationSession()
    {
    }

    public ConversationSession(Guid userId, Guid? cvProfileId = null, string? locale = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A conversation must belong to a user.", nameof(userId));
        }

        if (cvProfileId == Guid.Empty)
        {
            throw new ArgumentException("A CV profile ID cannot be empty.", nameof(cvProfileId));
        }

        UserId = userId;
        CvProfileId = cvProfileId;
        Locale = NormalizeLocale(locale);
        StartedAtUtc = DateTimeOffset.UtcNow;
        Status = ConversationSessionStatus.Active;
    }

    public Guid UserId { get; private set; }

    [JsonIgnore]
    public User? User { get; private set; }

    public Guid? CvProfileId { get; private set; }

    [JsonIgnore]
    public CvProfile? CvProfile { get; private set; }

    public string Locale { get; private set; } = "en";
    public ConversationSessionStatus Status { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? LastActivityAtUtc { get; private set; }
    public Guid? CurrentQuestionStateId { get; private set; }

    [JsonIgnore]
    public QuestionState? CurrentQuestionState { get; private set; }

    [JsonIgnore]
    public IReadOnlyCollection<ConversationMessage> Messages => _messages.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<QuestionState> QuestionStates => _questionStates.AsReadOnly();

    public ConversationMessage AddMessage(
        ConversationMessageRole role,
        string content,
        ConversationMessageType type = ConversationMessageType.Text,
        string? audioContentType = null,
        Guid? actorId = null)
    {
        EnsureActive();
        var message = new ConversationMessage(
            this, role, content, type, _messages.Count, audioContentType);
        _messages.Add(message);
        LastActivityAtUtc = message.CreatedAtUtc;
        MarkUpdated(actorId);
        return message;
    }

    public QuestionState AddQuestion(string key, string prompt, int order)
    {
        EnsureActive();
        var normalizedKey = CvProfile.RequiredText(key, nameof(key), 100).ToLowerInvariant();
        if (_questionStates.Any(question =>
                string.Equals(question.Key, normalizedKey, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("A question with this key already exists in the conversation.");
        }

        var state = new QuestionState(this, key, prompt, order);
        _questionStates.Add(state);
        if (CurrentQuestionState is null || state.Order < CurrentQuestionState.Order)
        {
            CurrentQuestionState = state;
            CurrentQuestionStateId = state.Id;
        }

        MarkUpdated(null);
        return state;
    }

    public void SetCurrentQuestion(Guid questionStateId)
    {
        EnsureActive();
        var question = _questionStates.FirstOrDefault(item => item.Id == questionStateId)
            ?? throw new DomainException("The question does not belong to this conversation.");
        if (question.Status is QuestionStatus.Answered or QuestionStatus.Skipped)
        {
            throw new DomainException("A completed question cannot be set as the current question.");
        }

        CurrentQuestionState = question;
        CurrentQuestionStateId = question.Id;
        MarkUpdated(null);
    }

    public void AttachCvProfile(CvProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        EnsureActive();
        if (profile.OwnerId != UserId)
        {
            throw new DomainException("A conversation can only be linked to a profile owned by its user.");
        }

        CvProfile = profile;
        CvProfileId = profile.Id;
        MarkUpdated(null);
    }

    public void Complete(Guid? actorId = null)
    {
        EnsureActive();
        Status = ConversationSessionStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        CurrentQuestionState = null;
        CurrentQuestionStateId = null;
        LastActivityAtUtc = CompletedAtUtc;
        MarkUpdated(actorId);
    }

    public void Abandon(Guid? actorId = null)
    {
        EnsureActive();
        Status = ConversationSessionStatus.Abandoned;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        CurrentQuestionState = null;
        CurrentQuestionStateId = null;
        LastActivityAtUtc = CompletedAtUtc;
        MarkUpdated(actorId);
    }

    public void Reopen(Guid? actorId = null)
    {
        if (Status == ConversationSessionStatus.Active)
        {
            return;
        }

        Status = ConversationSessionStatus.Active;
        CompletedAtUtc = null;
        CurrentQuestionState = _questionStates
            .Where(question => question.Status is QuestionStatus.Pending or QuestionStatus.Asked)
            .OrderBy(question => question.Order)
            .FirstOrDefault();
        CurrentQuestionStateId = CurrentQuestionState?.Id;
        LastActivityAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated(actorId);
    }

    internal void OnQuestionCompleted(QuestionState question)
    {
        if (CurrentQuestionStateId != question.Id)
        {
            return;
        }

        CurrentQuestionState = _questionStates
            .Where(item => item.Id != question.Id
                && item.Status is QuestionStatus.Pending or QuestionStatus.Asked)
            .OrderBy(item => item.Order)
            .FirstOrDefault();
        CurrentQuestionStateId = CurrentQuestionState?.Id;
        LastActivityAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated(null);
    }

    private void EnsureActive()
    {
        if (Status != ConversationSessionStatus.Active)
        {
            throw new DomainException("The conversation is not active.");
        }
    }

    private static string NormalizeLocale(string? locale)
    {
        var normalized = string.IsNullOrWhiteSpace(locale) ? "en" : locale.Trim();
        if (normalized.Length > 35)
        {
            throw new ArgumentOutOfRangeException(nameof(locale), "A locale identifier cannot exceed 35 characters.");
        }

        return normalized;
    }
}
