using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Services;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Documents.Commands.UploadDocument;

public sealed class UploadDocumentCommandHandler(
    IDocumentTextExtractor textExtractor,
    IAiService aiService,
    WorkspaceService workspaceService) : IRequestHandler<UploadDocumentCommand, UploadedDocument>
{
    public async Task<UploadedDocument> Handle(
        UploadDocumentCommand request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var stream = new MemoryStream(request.Content, writable: false);
        var extension = Path.GetExtension(Path.GetFileName(request.FileName));
        var extractedText = textExtractor.ExtractText(stream, extension);
        var fileName = Path.GetFileName(request.FileName);
        workspaceService.AddDocument(fileName, extractedText);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            const string emptyTextError = "No readable text was found in the document. You can still add CV details manually.";
            workspaceService.SetAiInsights("failed", emptyTextError);
            return new UploadedDocument(fileName, extractedText, AiError: emptyTextError);
        }

        var result = await aiService.ExtractCvAsync(extractedText, cancellationToken);
        if (result.Value is not { } analysis || string.IsNullOrWhiteSpace(analysis.Profile.FullName))
        {
            var error = result.Error
                ?? "The AI could not identify a candidate name. Extracted text is available for manual editing.";
            workspaceService.SetAiInsights("failed", error);
            return new UploadedDocument(fileName, extractedText, AiError: error);
        }

        var profile = analysis.Profile;
        workspaceService.UpdateProfile(profile);
        workspaceService.SetAiInsights(
            "success",
            null,
            analysis.MissingFields,
            analysis.NextQuestion,
            analysis.SuggestedImprovements,
            analysis.TemplateRecommendation);
        workspaceService.SelectTemplate(analysis.TemplateRecommendation);
        return new UploadedDocument(fileName, extractedText, profile);
    }
}
