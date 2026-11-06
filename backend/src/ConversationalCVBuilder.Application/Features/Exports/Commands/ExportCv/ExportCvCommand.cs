using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Exports.Commands.ExportCv;

public sealed record ExportCvCommand(
    string Format,
    Guid? CvProfileId = null,
    string? TemplateKey = null) : IRequest<ExportedCvFile>;
