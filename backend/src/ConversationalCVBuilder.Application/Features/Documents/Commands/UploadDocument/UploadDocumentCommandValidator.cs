using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.Documents.Commands.UploadDocument;

public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private const int MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> SupportedExtensions = new(
        [".pdf", ".docx", ".pptx", ".txt"],
        StringComparer.OrdinalIgnoreCase);

    public UploadDocumentCommandValidator()
    {
        RuleFor(command => command.FileName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(fileName => SupportedExtensions.Contains(Path.GetExtension(Path.GetFileName(fileName))))
            .WithMessage("Only PDF, DOCX, PPTX, and TXT files are supported.");
        RuleFor(command => command.Content)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(content => content is { Length: > 0 and <= MaxFileSizeBytes })
            .WithMessage("The uploaded file must be non-empty and 10 MB or smaller.");
    }
}
