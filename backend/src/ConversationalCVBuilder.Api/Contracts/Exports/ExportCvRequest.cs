using ConversationalCVBuilder.Application.Contracts;

namespace ConversationalCVBuilder.Api.Contracts.Exports;

public sealed record ExportCvRequest(
    Guid? CvProfileId = null,
    string? TemplateKey = null,
    CvDesignConfiguration? DesignConfiguration = null);
