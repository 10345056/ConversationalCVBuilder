using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Documents.Commands.UploadDocument;

public sealed record UploadDocumentCommand(
    string FileName,
    byte[] Content,
    string? ContentType = null) : IRequest<UploadedDocument>;
