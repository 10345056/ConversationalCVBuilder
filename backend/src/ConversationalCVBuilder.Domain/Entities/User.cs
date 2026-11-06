using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Common;
using ConversationalCVBuilder.Domain.ValueObjects;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class User : AggregateRoot
{
    private readonly List<ConversationSession> _conversationSessions = [];
    private readonly List<CvProfile> _cvProfiles = [];

    private User()
    {
    }

    public User(
        EmailAddress email,
        string displayName,
        string? externalIdentityProvider = null,
        string? externalSubject = null)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        DisplayName = CvProfile.RequiredText(displayName, nameof(displayName), 160);
        SetExternalIdentity(externalIdentityProvider, externalSubject);
        IsActive = true;
        EmailVerified = false;
        LastSeenAtUtc = DateTimeOffset.UtcNow;
    }

    public EmailAddress Email { get; private set; } = null!;
    public string NormalizedEmail => Email.Value.ToUpperInvariant();
    public string DisplayName { get; private set; } = string.Empty;
    public string? ExternalIdentityProvider { get; private set; }
    public string? ExternalSubject { get; private set; }
    public bool EmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? LastSeenAtUtc { get; private set; }

    [JsonIgnore]
    public IReadOnlyCollection<CvProfile> CvProfiles => _cvProfiles.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<ConversationSession> ConversationSessions => _conversationSessions.AsReadOnly();

    public CvProfile CreateCvProfile(string title = "CV", Guid? actorId = null)
    {
        EnsureActive();
        var profile = new CvProfile(Id, title);
        _cvProfiles.Add(profile);
        MarkUpdated(actorId);
        return profile;
    }

    public ConversationSession StartConversation(CvProfile? profile = null, string? locale = null)
    {
        EnsureActive();
        if (profile is not null && profile.OwnerId != Id)
        {
            throw new DomainException("A user can only start a conversation for their own CV profile.");
        }

        var session = new ConversationSession(Id, profile?.Id, locale);
        if (profile is not null)
        {
            session.AttachCvProfile(profile);
        }

        _conversationSessions.Add(session);
        LastSeenAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated(null);
        return session;
    }

    public void UpdateProfile(string displayName, EmailAddress email, Guid? actorId = null)
    {
        DisplayName = CvProfile.RequiredText(displayName, nameof(displayName), 160);
        Email = email ?? throw new ArgumentNullException(nameof(email));
        MarkUpdated(actorId);
    }

    public void MarkEmailVerified(Guid? actorId = null)
    {
        if (EmailVerified)
        {
            return;
        }

        EmailVerified = true;
        MarkUpdated(actorId);
    }

    public void RecordSignIn(Guid? actorId = null)
    {
        EnsureActive();
        LastSeenAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated(actorId);
    }

    public void Deactivate(Guid? actorId = null)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkUpdated(actorId);
    }

    public void Reactivate(Guid? actorId = null)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkUpdated(actorId);
    }

    private void SetExternalIdentity(string? provider, string? subject)
    {
        var normalizedProvider = CvProfile.OptionalText(provider, 80);
        var normalizedSubject = CvProfile.OptionalText(subject, 200);
        if ((normalizedProvider is null) != (normalizedSubject is null))
        {
            throw new ArgumentException("An external identity provider and subject must be supplied together.");
        }

        ExternalIdentityProvider = normalizedProvider;
        ExternalSubject = normalizedSubject;
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainException("The user account is inactive.");
        }
    }
}
