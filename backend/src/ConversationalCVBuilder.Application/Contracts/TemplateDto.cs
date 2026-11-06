namespace ConversationalCVBuilder.Application.Contracts;

public sealed record TemplateDto(
    string Key,
    string Name,
    string Description,
    string Category,
    string AccentColor,
    int DisplayOrder);
