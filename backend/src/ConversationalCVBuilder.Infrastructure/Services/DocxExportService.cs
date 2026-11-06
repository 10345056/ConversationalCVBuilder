using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Domain.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace ConversationalCVBuilder.Infrastructure.Services;

public sealed class DocxExportService : IDocxExporter
{
    private const string Primary = "00A9E0";
    private const string Secondary = "003B5C";
    private const string LightBlue = "F5FBFF";

    public byte[] Export(CvProfile profile, string templateId)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var generatedAt = DateTimeOffset.UtcNow;
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = document.AddMainDocumentPart();
            var body = new Body();
            mainPart.Document = new Document(body);
            document.PackageProperties.Creator = "Conversational CV Builder";
            document.PackageProperties.Title = $"NTT DATA CV - {profile.FullName}";

            AddLogo(mainPart, body);
            AddParagraph(body, profile.FullName, bold: true, fontSize: "34", color: Secondary);
            AddParagraph(body, profile.TargetRole, fontSize: "24", color: Primary);
            AddParagraph(body, string.Join("  |  ", new[]
            {
                profile.Email,
                profile.Phone,
                profile.Location,
                profile.PersonalInformation.Website?.ToString(),
                profile.PersonalInformation.LinkedInUrl?.ToString()
            }.Where(value => !string.IsNullOrWhiteSpace(value))));

            AddSection(body, "Professional Summary", profile.Summary);
            AddSection(body, "Skills", string.Join("  •  ", profile.SkillItems
                .OrderBy(item => item.DisplayOrder)
                .Select(item => item.Name)));
            AddSection(body, "Certifications", string.Join(Environment.NewLine, profile.Certifications
                .OrderBy(item => item.DisplayOrder)
                .Select(item => JoinNonEmpty(item.Name, item.IssuingOrganization, item.CredentialId))));
            AddSection(body, "Experience", string.Join(Environment.NewLine + Environment.NewLine,
                profile.WorkExperiences.OrderBy(item => item.DisplayOrder).Select(item =>
                    $"{JoinNonEmpty(item.PositionTitle, item.EmployerName, FormatDates(item.StartDate, item.EndDate, item.IsCurrent))}{Environment.NewLine}{item.Description}".Trim())));
            AddSection(body, "Projects", string.Join(Environment.NewLine + Environment.NewLine,
                profile.Projects.OrderBy(item => item.DisplayOrder).Select(item =>
                    $"{item.Name}{Environment.NewLine}{JoinNonEmpty(item.Description, item.Technologies.Count == 0 ? null : $"Technologies: {string.Join(", ", item.Technologies)}")}".Trim())));
            AddSection(body, "Education", string.Join(Environment.NewLine + Environment.NewLine,
                profile.EducationEntries.OrderBy(item => item.DisplayOrder).Select(item =>
                    $"{JoinNonEmpty(item.Qualification, item.InstitutionName, item.FieldOfStudy, FormatDates(item.StartDate, item.EndDate, item.IsInProgress))}{Environment.NewLine}{item.Description}".Trim())));
            AddSection(body, "Languages", string.Join("  •  ", profile.Languages
                .OrderBy(item => item.DisplayOrder)
                .Select(item => $"{item.Name} ({item.Proficiency})")));
            AddSection(body, "Achievements", string.Join(Environment.NewLine, profile.Achievements));

            var footerPart = mainPart.AddNewPart<FooterPart>();
            footerPart.Footer = CreateFooter(generatedAt);
            var sectionProperties = new SectionProperties(
                new FooterReference
                {
                    Type = HeaderFooterValues.Default,
                    Id = mainPart.GetIdOfPart(footerPart)
                },
                new PageSize { Width = 11906, Height = 16838 },
                new PageMargin { Top = 850, Right = 850, Bottom = 850, Left = 850, Header = 425, Footer = 425, Gutter = 0 });
            body.Append(sectionProperties);

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static void AddLogo(MainDocumentPart mainPart, Body body)
    {
        var imagePart = mainPart.AddImagePart(ImagePartType.Png);
        using (var logoStream = new MemoryStream(NttBrandAssets.ReadLogo(), writable: false))
        {
            imagePart.FeedData(logoStream);
        }

        var relationshipId = mainPart.GetIdOfPart(imagePart);
        const long width = 2_400_000;
        const long height = 500_000;
        var picture = new PIC.Picture(
            new PIC.NonVisualPictureProperties(
                new PIC.NonVisualDrawingProperties { Id = 0U, Name = "NTT DATA Logo" },
                new PIC.NonVisualPictureDrawingProperties()),
            new PIC.BlipFill(
                new A.Blip { Embed = relationshipId },
                new A.Stretch(new A.FillRectangle())),
            new PIC.ShapeProperties(
                new A.Transform2D(
                    new A.Offset { X = 0L, Y = 0L },
                    new A.Extents { Cx = width, Cy = height }),
                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }));
        var drawing = new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = width, Cy = height },
                new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                new DW.DocProperties { Id = 1U, Name = "NTT DATA" },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(picture)
                    {
                        Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
                    }))
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            });
        body.Append(new Paragraph(new Run(drawing)));
    }

    private static Footer CreateFooter(DateTimeOffset generatedAt)
    {
        var paragraph = new Paragraph(
            new ParagraphProperties(
                new ParagraphBorders(new TopBorder
                {
                    Val = BorderValues.Single,
                    Color = "D8EAF3",
                    Size = 6,
                    Space = 6
                }),
                new Justification { Val = JustificationValues.Center }),
            new Run(new RunProperties(new FontSize { Val = "16" }, new Color { Val = Secondary }),
                new Text($"Generated using NTT DATA Conversational CV Builder · Generated by Conversational CV Builder · {generatedAt:yyyy-MM-dd} · Page ")),
            CreateField("PAGE"),
            new Run(new RunProperties(new FontSize { Val = "16" }, new Color { Val = Secondary }), new Text(" of ")),
            CreateField("NUMPAGES"));
        return new Footer(paragraph);
    }

    private static Run CreateField(string fieldName) =>
        new(
            new FieldChar { FieldCharType = FieldCharValues.Begin },
            new FieldCode($" {fieldName} "),
            new FieldChar { FieldCharType = FieldCharValues.End });

    private static void AddSection(Body body, string heading, string? value)
    {
        var headingParagraph = new Paragraph(
            new ParagraphProperties(
                new SpacingBetweenLines { Before = "180", After = "80" },
                new Shading { Val = ShadingPatternValues.Clear, Fill = LightBlue },
                new ParagraphBorders(new BottomBorder
                {
                    Val = BorderValues.Single,
                    Color = Primary,
                    Size = 8,
                    Space = 3
                })),
            new Run(
                new RunProperties(new Bold(), new Color { Val = Primary }, new FontSize { Val = "22" }),
                new Text(heading.ToUpperInvariant())));
        body.Append(headingParagraph);
        AddParagraph(
            body,
            string.IsNullOrWhiteSpace(value) ? CvExportMissingContent.For(heading) : value.Trim(),
            fontSize: "20",
            color: Secondary);
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

    private static void AddParagraph(
        Body body,
        string text,
        bool bold = false,
        string? fontSize = null,
        string? color = null)
    {
        var properties = new RunProperties();
        if (bold)
        {
            properties.Append(new Bold());
        }

        if (fontSize is not null)
        {
            properties.Append(new FontSize { Val = fontSize });
        }

        if (color is not null)
        {
            properties.Append(new Color { Val = color });
        }

        var run = new Run();
        if (properties.ChildElements.Count > 0)
        {
            run.Append(properties);
        }

        var lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < lines.Length; index++)
        {
            run.Append(new Text(lines[index].Trim()) { Space = SpaceProcessingModeValues.Preserve });
            if (index < lines.Length - 1)
            {
                run.Append(new Break());
            }
        }

        body.Append(new Paragraph(run));
    }
}
