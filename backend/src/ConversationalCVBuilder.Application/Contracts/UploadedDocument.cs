namespace ConversationalCVBuilder.Application.Contracts;

public sealed record UploadedDocument(
    string FileName,
    string ExtractedText,
    CvAiProfile? Profile = null,
    string? AiError = null);
