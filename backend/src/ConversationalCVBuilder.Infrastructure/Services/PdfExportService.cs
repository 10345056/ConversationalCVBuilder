using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ConversationalCVBuilder.Infrastructure.Services;

public sealed class PdfExportService : IPdfExporter
{
    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Export(
        CvProfile profile,
        string? templateKey = null,
        CvDesignConfiguration? designConfiguration = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var design = designConfiguration ?? new CvDesignConfiguration();
        var primary = design.PrimaryColor;
        var secondary = design.SecondaryColor;
        var lightColor = design.LightColor;
        var logo = design.ShowLogo ? NttBrandAssets.ReadLogo() : null;
        var generatedAt = DateTimeOffset.UtcNow;
        var contact = string.Join("  |  ", new[]
        {
            profile.Email,
            profile.Phone,
            profile.Location,
            profile.PersonalInformation.Website?.ToString(),
            profile.PersonalInformation.LinkedInUrl?.ToString()
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var sectionContent = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["summary"] = profile.Summary,
            ["skills"] = FormatSkills(profile.SkillItems
                .OrderBy(item => item.DisplayOrder)
                .Select(item => item.Name)),
            ["experience"] = string.Join(Environment.NewLine + Environment.NewLine,
                profile.WorkExperiences.OrderBy(item => item.DisplayOrder).Select(item =>
                    $"{JoinNonEmpty(item.PositionTitle, item.EmployerName, FormatDates(item.StartDate, item.EndDate, item.IsCurrent))}{Environment.NewLine}{item.Description}".Trim())),
            ["achievements"] = string.Join(Environment.NewLine, profile.Achievements),
            ["projects"] = string.Join(Environment.NewLine + Environment.NewLine,
                profile.Projects.OrderBy(item => item.DisplayOrder).Select(item =>
                    $"{item.Name}{Environment.NewLine}{JoinNonEmpty(item.Description, item.Technologies.Count == 0 ? null : $"Technologies: {string.Join(", ", item.Technologies)}")}".Trim())),
            ["certifications"] = string.Join(Environment.NewLine, profile.Certifications
                .OrderBy(item => item.DisplayOrder)
                .Select(item => JoinNonEmpty(item.Name, item.IssuingOrganization, item.CredentialId))),
            ["education"] = string.Join(Environment.NewLine + Environment.NewLine,
                profile.EducationEntries.OrderBy(item => item.DisplayOrder).Select(item =>
                    $"{JoinNonEmpty(item.Qualification, item.InstitutionName, item.FieldOfStudy, FormatDates(item.StartDate, item.EndDate, item.IsInProgress))}{Environment.NewLine}{item.Description}".Trim())),
            ["additional"] = string.Join("  •  ", profile.LanguageLabels
                .Concat(profile.AdditionalInformation)
                .Where(item => !string.IsNullOrWhiteSpace(item)))
        };
        var configuredSections = design.Sections.Count == 0
            ? CreateDefaultSections()
            : design.Sections;
        var orderedSections = configuredSections
            .Where(section => section.Enabled && sectionContent.ContainsKey(section.Id))
            .OrderBy(section => section.Order)
            .ToArray();

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(design.PageMargin);
                page.DefaultTextStyle(style => style.FontSize(design.FontSize).FontFamily(design.FontFamily).FontColor(secondary));
                page.Header().PaddingBottom(14).Background(lightColor).Padding(15).Row(row =>
                {
                    if (logo is not null)
                    {
                        row.ConstantItem(112).Height(34).Image(logo).FitArea();
                    }

                    row.RelativeItem().PaddingLeft(logo is not null ? 16 : 0).Column(header =>
                    {
                        header.Item().Text("PERSONAL INFORMATION").FontSize(design.FontSize - 2).Bold().FontColor(primary);
                        header.Item().Text(profile.FullName).FontSize(design.FontSize + 12).Bold().FontColor(secondary);
                        if (!string.IsNullOrWhiteSpace(profile.TargetRole))
                        {
                            header.Item().PaddingTop(2).Text(profile.TargetRole).FontSize(design.FontSize + 2).FontColor(primary);
                        }

                        if (!string.IsNullOrWhiteSpace(contact))
                        {
                            header.Item().PaddingTop(4).Text(contact).FontSize(8).FontColor(Colors.Grey.Darken2);
                        }
                        else
                        {
                            header.Item().PaddingTop(4).Text("Add confirmed email, phone, and location before sharing.")
                                .FontSize(8).FontColor(Colors.Grey.Darken2);
                        }
                    });
                });
                page.Content().PaddingTop(8).Column(content =>
                {
                    content.Spacing(design.SectionSpacing);
                    foreach (var section in orderedSections)
                    {
                        AddSection(content, section.Title, sectionContent[section.Id], design);
                    }

                    foreach (var section in design.CustomSections)
                    {
                        AddSection(content, section.Title, section.Content, design);
                    }
                });
                if (design.ShowFooter)
                {
                    page.Footer().PaddingTop(10).BorderTop(1).BorderColor(lightColor).Row(row =>
                    {
                        row.RelativeItem().Text(
                            $"NTT DATA Professional CV · {generatedAt:yyyy-MM-dd}")
                            .FontSize(design.FontSize - 3).FontColor(Colors.Grey.Darken1);
                        row.ConstantItem(85).AlignRight().DefaultTextStyle(style =>
                            style.FontSize(design.FontSize - 2).FontColor(secondary)).Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
                }
            });
        }).GeneratePdf();
    }

    private static List<CvDesignSection> CreateDefaultSections() =>
    [
        new("summary", "Professional Summary", true, 0),
        new("skills", "Technical Skills", true, 1),
        new("experience", "Professional Experience", true, 2),
        new("achievements", "Key Achievements", true, 3),
        new("projects", "Projects", true, 4),
        new("certifications", "Certifications", true, 5),
        new("education", "Education", true, 6),
        new("additional", "Additional Information", true, 7)
    ];

    private static void AddSection(
        ColumnDescriptor column,
        string heading,
        string? content,
        CvDesignConfiguration design)
    {
        var padding = design.Layout == "compact" ? 6 : 9;
        column.Item().Background(design.LightColor).BorderLeft(3).BorderColor(design.PrimaryColor).Padding(padding).Column(section =>
        {
            section.Item().Text(heading.ToUpperInvariant()).FontSize(design.FontSize).Bold().FontColor(design.PrimaryColor);
            var value = string.IsNullOrWhiteSpace(content) ? CvExportMissingContent.For(heading) : content.Trim();
            var lines = value.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            section.Item().PaddingTop(4).Text(string.Join(Environment.NewLine, lines.Select(FormatBullet)))
                .FontSize(design.FontSize - 1).FontFamily(design.FontFamily).FontColor(design.SecondaryColor);
        });
    }

    private static string FormatBullet(string line)
    {
        var trimmed = line.Trim();
        return trimmed.StartsWith("•", StringComparison.Ordinal) || trimmed.StartsWith("- ", StringComparison.Ordinal)
            ? $"• {trimmed.TrimStart('•', '-', ' ')}"
            : trimmed;
    }

    private static string FormatSkills(IEnumerable<string> skills)
    {
        var items = skills.ToArray();
        var resumeSkills = items.Where(skill => !skill.StartsWith("Suggested (verify): ", StringComparison.OrdinalIgnoreCase)).ToArray();
        var suggestedSkills = items.Where(skill => skill.StartsWith("Suggested (verify): ", StringComparison.OrdinalIgnoreCase))
            .Select(skill => skill["Suggested (verify): ".Length..])
            .ToArray();
        return string.Join(Environment.NewLine, new[]
        {
            resumeSkills.Length == 0 ? null : $"Resume-listed skills: {string.Join(", ", resumeSkills)}",
            suggestedSkills.Length == 0 ? null : $"Suggested skills to review: {string.Join(", ", suggestedSkills)}",
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string JoinNonEmpty(params string?[] values) =>
        string.Join(" | ", values.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string FormatDates(DateOnly? start, DateOnly? end, bool current)
    {
        if (!start.HasValue && !end.HasValue && !current)
        {
            return string.Empty;
        }

        var startText = start?.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "Date not specified";
        var endText = current
            ? "Present"
            : end?.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        return string.IsNullOrWhiteSpace(endText) ? startText : $"{startText} – {endText}";
    }
}
