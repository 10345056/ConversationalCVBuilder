namespace ConversationalCVBuilder.Application.Contracts;

public sealed record UploadedDocument(
    string FileName,
    string ExtractedText,
    CvAiProfile? Profile = null,
    string? AiError = null,
    string Status = "success",
    string Message = "",
    long FileSizeBytes = 0,
    string ContentType = "",
    int ExtractedCharacterCount = 0);
