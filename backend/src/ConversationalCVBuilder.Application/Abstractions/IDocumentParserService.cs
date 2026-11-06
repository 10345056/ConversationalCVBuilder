namespace ConversationalCVBuilder.Application.Abstractions;

public interface IDocumentParserService
{
    Task<string> ExtractTextAsync(
        Stream file,
        string fileName,
        CancellationToken cancellationToken = default);
}
