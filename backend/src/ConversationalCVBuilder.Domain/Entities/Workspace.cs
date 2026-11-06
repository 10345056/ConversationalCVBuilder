using System.Text.Json.Serialization;
using ConversationalCVBuilder.Domain.Enums;
using ConversationalCVBuilder.Domain.ValueObjects;

namespace ConversationalCVBuilder.Domain.Entities;

public sealed class Workspace
{
    private readonly User _user;

    public Workspace()
    {
        _user = new User(EmailAddress.Create("local@cvbuilder.invalid"), "CV Builder User");
        Profile = _user.CreateCvProfile();
        Conversation = _user.StartConversation(Profile);
        AddMessage(
            "assistant",
            "Hi! I’ll help you build a clear, tailored CV. What’s your full name?");
    }

    public CvProfile Profile { get; private set; }
    public string CurrentStep { get; private set; } = "fullName";
    public string SelectedTemplateId => Profile.SelectedTemplateKey;
    public string UploadedDocumentName { get; private set; } = string.Empty;
    public string SourceText { get; private set; } = string.Empty;
    public string AiStatus { get; private set; } = "not-run";
    public string AiError { get; private set; } = string.Empty;
    public string AiNextQuestion { get; private set; } = string.Empty;
    public string AiTemplateRecommendation { get; private set; } = "modern";
    public IReadOnlyList<string> AiMissingFields { get; private set; } = [];
    public IReadOnlyList<string> AiSuggestedImprovements { get; private set; } = [];
    public IReadOnlyList<WorkspaceMessage> Messages => Conversation.Messages
        .Select(message => new WorkspaceMessage(
            message.Role.ToString().ToLowerInvariant(),
            message.Content,
            message.CreatedAtUtc))
        .ToArray();

    [JsonIgnore]
    public ConversationSession Conversation { get; private set; }

    public CvProfile CreateProfile(string title)
    {
        Profile = _user.CreateCvProfile(title);
        Conversation = _user.StartConversation(Profile);
        CurrentStep = "fullName";
        UploadedDocumentName = string.Empty;
        SourceText = string.Empty;
        AiStatus = "not-run";
        AiError = string.Empty;
        AiNextQuestion = string.Empty;
        AiTemplateRecommendation = "modern";
        AiMissingFields = [];
        AiSuggestedImprovements = [];
        AddMessage(
            "assistant",
            "Hi! I’ll help you build a clear, tailored CV. What’s your full name?");
        return Profile;
    }

    public void AddMessage(string role, string content)
    {
        var messageRole = role.Trim().ToLowerInvariant() switch
        {
            "user" => ConversationMessageRole.User,
            "assistant" => ConversationMessageRole.Assistant,
            "system" => ConversationMessageRole.System,
            _ => throw new ArgumentException("Message role must be user, assistant, or system.", nameof(role))
        };
        Conversation.AddMessage(messageRole, content);
    }

    public void SetCurrentStep(string step)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(step);
        CurrentStep = step.Trim();
    }

    public void SelectTemplate(string templateKey)
    {
        Profile.SelectTemplate(templateKey);
    }

    public void SetDocumentReference(string fileName, string extractedText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        UploadedDocumentName = Path.GetFileName(fileName);
        SourceText = extractedText ?? throw new ArgumentNullException(nameof(extractedText));
        CurrentStep = "document-uploaded";
        AiStatus = "pending";
        AiError = string.Empty;
        AiNextQuestion = string.Empty;
        AiTemplateRecommendation = "modern";
        AiMissingFields = [];
        AiSuggestedImprovements = [];
        AddMessage(
            "assistant",
            $"I read “{UploadedDocumentName}”. The extracted text is ready to use as reference while we build your CV.");
    }

    public void SetAiInsights(
        string status,
        string? error,
        IEnumerable<string>? missingFields = null,
        string? nextQuestion = null,
        IEnumerable<string>? suggestedImprovements = null,
        string? templateRecommendation = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        AiStatus = status.Trim();
        AiError = error?.Trim() ?? string.Empty;
        AiMissingFields = (missingFields ?? []).ToArray();
        AiNextQuestion = nextQuestion?.Trim() ?? string.Empty;
        AiSuggestedImprovements = (suggestedImprovements ?? []).ToArray();
        AiTemplateRecommendation = templateRecommendation is "classic" or "modern" or "creative"
            ? templateRecommendation
            : "modern";
    }
}

public sealed record WorkspaceMessage(string Role, string Content, DateTimeOffset Timestamp);
