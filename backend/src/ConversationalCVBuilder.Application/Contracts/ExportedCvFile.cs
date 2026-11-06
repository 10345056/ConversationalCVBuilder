namespace ConversationalCVBuilder.Application.Contracts;

public sealed record ExportedCvFile(string FileName, string ContentType, byte[] Content);
