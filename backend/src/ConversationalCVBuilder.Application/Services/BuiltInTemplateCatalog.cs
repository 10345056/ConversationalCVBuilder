using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;

namespace ConversationalCVBuilder.Application.Services;

public sealed class BuiltInTemplateCatalog : ITemplateCatalog
{
    private static readonly IReadOnlyList<TemplateDto> Templates =
    [
        new("classic", "Classic", "A clean, traditional layout for a broad range of roles.", "Professional", "#1D4ED8", 1),
        new("modern", "Modern", "A contemporary layout with clear visual hierarchy.", "Professional", "#0E7490", 2),
        new("creative", "Creative", "A distinctive layout for portfolio-led applications.", "Creative", "#7C3AED", 3)
    ];

    public IReadOnlyList<TemplateDto> GetAll() => Templates;
}
