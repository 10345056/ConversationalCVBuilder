using System.Text;
using ConversationalCVBuilder.Application.Abstractions;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;

namespace ConversationalCVBuilder.Infrastructure.Services;

public sealed class DocumentParserService : IDocumentParserService, IDocumentTextExtractor
{
    private const int MaximumDocumentSizeBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> SupportedExtensions = new(
        [".pdf", ".docx", ".pptx"],
        StringComparer.OrdinalIgnoreCase);
    public string ExtractText(Stream fileStream, string extension)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        var normalizedExtension = extension.StartsWith(".", StringComparison.Ordinal)
            ? extension
            : $".{extension}";
        EnsureSupported(normalizedExtension);
        using var bufferedStream = CopyToBoundedMemoryStream(fileStream);
        return ExtractBufferedText(bufferedStream, normalizedExtension);
    }

    public async Task<string> ExtractTextAsync(
        Stream file,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (!file.CanRead)
        {
            throw new ArgumentException("The document stream must be readable.", nameof(file));
        }

        var extension = Path.GetExtension(Path.GetFileName(fileName));
        EnsureSupported(extension);
        await using var bufferedStream = new MemoryStream();
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await file.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (bufferedStream.Length + bytesRead > MaximumDocumentSizeBytes)
            {
                throw new InvalidDataException("Documents must be 10 MB or smaller.");
            }

            await bufferedStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        bufferedStream.Position = 0;
        return ExtractBufferedText(bufferedStream, extension);
    }

    private static MemoryStream CopyToBoundedMemoryStream(Stream source)
    {
        var copy = new MemoryStream();
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (copy.Length + bytesRead > MaximumDocumentSizeBytes)
            {
                copy.Dispose();
                throw new InvalidDataException("Documents must be 10 MB or smaller.");
            }

            copy.Write(buffer, 0, bytesRead);
        }

        copy.Position = 0;
        return copy;
    }

    private static void EnsureSupported(string extension)
    {
        if (!SupportedExtensions.Contains(extension))
        {
            throw new InvalidDataException("Only PDF, DOCX, and PPTX files are supported.");
        }
    }

    private static string ExtractBufferedText(Stream stream, string extension)
    {
        stream.Position = 0;
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => ExtractPdf(stream),
            ".docx" => ExtractWord(stream),
            ".pptx" => ExtractPresentation(stream),
            _ => throw new InvalidDataException("Only PDF, DOCX, and PPTX files are supported.")
        };
    }

    private static string ExtractPdf(Stream stream)
    {
        try
        {
            using var document = PdfDocument.Open(stream);
            return string.Join(Environment.NewLine, document.GetPages().Select(page => page.Text));
        }
        catch (PdfDocumentFormatException exception)
        {
            throw new InvalidDataException("The uploaded PDF file is invalid or unreadable.", exception);
        }
    }

    private static string ExtractWord(Stream stream)
    {
        try
        {
            using var document = WordprocessingDocument.Open(stream, false);
            return document.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
        }
        catch (OpenXmlPackageException exception)
        {
            throw new InvalidDataException("The uploaded DOCX file is invalid or unreadable.", exception);
        }
        catch (FileFormatException exception)
        {
            throw new InvalidDataException("The uploaded DOCX file is invalid or unreadable.", exception);
        }
    }

    private static string ExtractPresentation(Stream stream)
    {
        try
        {
            using var document = PresentationDocument.Open(stream, false);
            var text = new StringBuilder();
            foreach (var slide in document.PresentationPart?.SlideParts ?? [])
            {
                if (slide.Slide is not { } slideContent)
                {
                    continue;
                }

                foreach (var paragraph in slideContent.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    var line = string.Concat(paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>()
                        .Select(item => item.Text));
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        text.AppendLine(line);
                    }
                }
            }

            return text.ToString();
        }
        catch (OpenXmlPackageException exception)
        {
            throw new InvalidDataException("The uploaded PPTX file is invalid or unreadable.", exception);
        }
        catch (FileFormatException exception)
        {
            throw new InvalidDataException("The uploaded PPTX file is invalid or unreadable.", exception);
        }
    }
}
