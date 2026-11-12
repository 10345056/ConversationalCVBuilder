using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.Exports.Commands.ExportCv;

public sealed class ExportCvCommandValidator : AbstractValidator<ExportCvCommand>
{
    public ExportCvCommandValidator()
    {
        RuleFor(command => command.Format)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(format => format.Equals("docx", StringComparison.OrdinalIgnoreCase)
                || format.Equals("json", StringComparison.OrdinalIgnoreCase)
                || format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Supported export formats are DOCX, PDF, and JSON.");
        RuleFor(command => command.CvProfileId)
            .Cascade(CascadeMode.Stop)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("CV profile ID cannot be empty.");
        RuleFor(command => command.TemplateKey)
            .Cascade(CascadeMode.Stop)
            .Must(templateKey => templateKey is null
                || templateKey.Equals("classic", StringComparison.OrdinalIgnoreCase)
                || templateKey.Equals("modern", StringComparison.OrdinalIgnoreCase)
                || templateKey.Equals("creative", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Choose a supported CV template.");
        When(command => command.DesignConfiguration is not null, () =>
            RuleFor(command => command.DesignConfiguration!)
                .SetValidator(new CvDesignConfigurationValidator()));
    }
}
