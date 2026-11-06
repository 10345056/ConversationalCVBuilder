namespace ConversationalCVBuilder.Api.Contracts.Exports;

public sealed record ExportCvRequest(Guid? CvProfileId = null, string? TemplateKey = null);
