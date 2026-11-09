using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ConversationalCVBuilder.Infrastructure.Services;

public sealed class PdfExportService : IPdfExporter
{
    private const string Primary = "#00A9E0";
    private const string Secondary = "#003B5C";
    private const string LightBlue = "#F5FBFF";

    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Export(CvProfile profile, string? templateKey = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var logo = NttBrandAssets.ReadLogo();
        var generatedAt = DateTimeOffset.UtcNow;
        var contact = string.Join("  |  ", new[]
        {
            profile.Email,
            profile.Phone,
            profile.Location,
            profile.PersonalInformation.Website?.ToString(),
            profile.PersonalInformation.LinkedInUrl?.ToString()
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(42);
                page.DefaultTextStyle(style => style.FontSize(10).FontColor(Secondary));
                page.Header().PaddingBottom(14).Background(LightBlue).Padding(15).Row(row =>
                {
                    row.ConstantItem(112).Height(34).Image(logo).FitArea();
                    row.RelativeItem().PaddingLeft(16).Column(header =>
                    {
                        header.Item().Text("PERSONAL INFORMATION").FontSize(8).Bold().FontColor(Primary);
                        header.Item().Text(profile.FullName).FontSize(22).Bold().FontColor(Secondary);
                        if (!string.IsNullOrWhiteSpace(profile.TargetRole))
                        {
                            header.Item().PaddingTop(2).Text(profile.TargetRole).FontSize(12).FontColor(Primary);
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
                    content.Spacing(8);
                    AddSection(content, "Professional Summary", profile.Summary);
                    AddSection(content, "Technical Skills", FormatSkills(profile.SkillItems
                        .OrderBy(item => item.DisplayOrder)
                        .Select(item => item.Name)));
                    AddSection(content, "Professional Experience", string.Join(Environment.NewLine + Environment.NewLine,
                        profile.WorkExperiences.OrderBy(item => item.DisplayOrder).Select(item =>
                            $"{JoinNonEmpty(item.PositionTitle, item.EmployerName, FormatDates(item.StartDate, item.EndDate, item.IsCurrent))}{Environment.NewLine}{item.Description}".Trim())));
                    AddSection(content, "Key Achievements", string.Join(Environment.NewLine, profile.Achievements));
                    AddSection(content, "Projects", string.Join(Environment.NewLine + Environment.NewLine,
                        profile.Projects.OrderBy(item => item.DisplayOrder).Select(item =>
                            $"{item.Name}{Environment.NewLine}{JoinNonEmpty(item.Description, item.Technologies.Count == 0 ? null : $"Technologies: {string.Join(", ", item.Technologies)}")}".Trim())));
                    AddSection(content, "Certifications", string.Join(Environment.NewLine, profile.Certifications
                        .OrderBy(item => item.DisplayOrder)
                        .Select(item => JoinNonEmpty(item.Name, item.IssuingOrganization, item.CredentialId))));
                    AddSection(content, "Education", string.Join(Environment.NewLine + Environment.NewLine,
                        profile.EducationEntries.OrderBy(item => item.DisplayOrder).Select(item =>
                            $"{JoinNonEmpty(item.Qualification, item.InstitutionName, item.FieldOfStudy, FormatDates(item.StartDate, item.EndDate, item.IsInProgress))}{Environment.NewLine}{item.Description}".Trim())));
                    AddSection(content, "Additional Information", string.Join("  •  ", profile.LanguageLabels
                        .Concat(profile.AdditionalInformation)
                        .Where(item => !string.IsNullOrWhiteSpace(item))));
                });
                page.Footer().PaddingTop(10).BorderTop(1).BorderColor("#D8EAF3").Row(row =>
                {
                    row.RelativeItem().Text(
                        $"NTT DATA Professional CV · {generatedAt:yyyy-MM-dd}")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                    row.ConstantItem(85).AlignRight().DefaultTextStyle(style =>
                        style.FontSize(8).FontColor(Secondary)).Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            });
        }).GeneratePdf();
    }

    private static void AddSection(ColumnDescriptor column, string heading, string? content)
    {
        column.Item().Background(LightBlue).BorderLeft(3).BorderColor(Primary).Padding(9).Column(section =>
        {
            section.Item().Text(heading.ToUpperInvariant()).FontSize(10).Bold().FontColor(Primary);
            var value = string.IsNullOrWhiteSpace(content) ? CvExportMissingContent.For(heading) : content.Trim();
            var lines = value.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            section.Item().PaddingTop(4).Text(string.Join(Environment.NewLine, lines.Select(FormatBullet)))
                .FontSize(9).FontColor(Secondary);
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
