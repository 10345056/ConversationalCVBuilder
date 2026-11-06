using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.Templates.Commands.SelectTemplate;

public sealed class SelectTemplateCommandValidator : AbstractValidator<SelectTemplateCommand>
{
    private static readonly HashSet<string> SupportedTemplates = new(
        ["classic", "modern", "creative"],
        StringComparer.OrdinalIgnoreCase);

    public SelectTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateKey)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(templateKey => SupportedTemplates.Contains(templateKey))
            .WithMessage("Choose a supported CV template.");
    }
}
