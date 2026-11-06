namespace ConversationalCVBuilder.Application.Contracts;

public sealed record ConversationDto(
    Guid Id,
    string Status,
    string CurrentStep,
    IReadOnlyList<ConversationMessageDto> Messages,
    IReadOnlyList<QuestionStateDto> Questions);

public sealed record ConversationMessageDto(
    Guid Id,
    string Role,
    string Type,
    string Content,
    DateTimeOffset CreatedAtUtc);

public sealed record QuestionStateDto(
    Guid Id,
    string Key,
    string Prompt,
    int Order,
    string Status,
    string? Answer);
