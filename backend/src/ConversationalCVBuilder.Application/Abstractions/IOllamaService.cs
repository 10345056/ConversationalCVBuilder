namespace ConversationalCVBuilder.Application.Abstractions;

public interface IOllamaService
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
