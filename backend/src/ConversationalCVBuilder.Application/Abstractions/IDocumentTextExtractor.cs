namespace ConversationalCVBuilder.Application.Abstractions;

public interface IDocumentTextExtractor
{
    string ExtractText(Stream fileStream, string extension);
}
