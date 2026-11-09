using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Services;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Documents.Commands.UploadDocument;

public sealed class UploadDocumentCommandHandler(
    IDocumentTextExtractor textExtractor,
    WorkspaceService workspaceService) : IRequestHandler<UploadDocumentCommand, UploadedDocument>
{
    public Task<UploadedDocument> Handle(
        UploadDocumentCommand request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var stream = new MemoryStream(request.Content, writable: false);
        var extension = Path.GetExtension(Path.GetFileName(request.FileName));
        var fileName = Path.GetFileName(request.FileName);
        var extractedText = string.Empty;
        string? extractionError = null;
        try
        {
            extractedText = textExtractor.ExtractText(stream, extension);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or ArgumentException)
        {
            extractionError = exception.Message;
        }

        workspaceService.AddDocument(fileName, extractedText);
        var draft = CvDraftBuilder.Build(extractedText);
        workspaceService.UpdateProfile(draft);
        workspaceService.SetAiInsights("pending", null, templateRecommendation: "modern");
        var partial = string.IsNullOrWhiteSpace(extractedText);
        var message = partial
            ? "Limited resume information is available. Review the draft and complete any missing details before sharing."
            : "Your resume details are ready. Prepare a CV draft to update the live preview.";
        return Task.FromResult(CreateResult(
            request,
            fileName,
            extractedText,
            draft,
            partial ? "partial" : "success",
            message,
            extractionError));
    }

    private static UploadedDocument CreateResult(
        UploadDocumentCommand request,
        string fileName,
        string extractedText,
        CvAiProfile profile,
        string status,
        string message,
        string? diagnostic) =>
        new(
            fileName,
            extractedText,
            profile,
            diagnostic,
            status,
            message,
            request.Content.LongLength,
            request.ContentType ?? string.Empty,
            extractedText.Length);
}
