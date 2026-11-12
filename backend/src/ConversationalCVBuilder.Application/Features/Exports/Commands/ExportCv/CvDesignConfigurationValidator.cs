using System.Text.RegularExpressions;
using ConversationalCVBuilder.Application.Contracts;
using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.Exports.Commands.ExportCv;

public sealed class CvDesignConfigurationValidator : AbstractValidator<CvDesignConfiguration>
{
    private static readonly string[] BuiltInSections =
    [
        "summary", "skills", "experience", "achievements",
        "projects", "certifications", "education", "additional"
    ];

    public CvDesignConfigurationValidator()
    {
        RuleFor(configuration => configuration.Id).MaximumLength(64);
        RuleFor(configuration => configuration.Name).NotEmpty().MaximumLength(60);
        RuleFor(configuration => configuration.PrimaryColor).Must(BeHexColor);
        RuleFor(configuration => configuration.SecondaryColor).Must(BeHexColor);
        RuleFor(configuration => configuration.LightColor).Must(BeHexColor);
        RuleFor(configuration => configuration.FontFamily)
            .Must(font => font is "Arial" or "Aptos" or "Georgia" or "Times New Roman");
        RuleFor(configuration => configuration.FontSize).InclusiveBetween(8, 14);
        RuleFor(configuration => configuration.PageMargin).InclusiveBetween(24, 72);
        RuleFor(configuration => configuration.SectionSpacing).InclusiveBetween(4, 20);
        RuleFor(configuration => configuration.Layout).Must(layout => layout is "standard" or "compact");
        RuleFor(configuration => configuration.Sections)
            .Must(sections => sections.Count <= BuiltInSections.Length
                && sections.Select(section => section.Id).Distinct(StringComparer.Ordinal).Count() == sections.Count
                && sections.All(section => BuiltInSections.Contains(section.Id, StringComparer.Ordinal)
                    && !string.IsNullOrWhiteSpace(section.Title)
                    && section.Title.Length <= 80));
        RuleFor(configuration => configuration.CustomSections)
            .Must(sections => sections.Count <= 8
                && sections.Select(section => section.Id).Distinct(StringComparer.Ordinal).Count() == sections.Count
                && sections.All(section => !string.IsNullOrWhiteSpace(section.Title)
                    && section.Title.Length <= 60
                    && section.Content.Length <= 4000));
    }

    private static bool BeHexColor(string? color) =>
        color is not null && Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant);
}
