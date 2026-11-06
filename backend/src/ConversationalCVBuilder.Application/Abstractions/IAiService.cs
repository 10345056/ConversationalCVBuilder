using ConversationalCVBuilder.Application.Contracts;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface IAiService
{
    Task<AiResult<CvAiAnalysis>> ExtractCvAsync(
        string extractedText,
        CancellationToken cancellationToken = default);

    Task<AiResult<CvAiAnalysis>> OptimizeCvAsync(
        CvAiProfile profile,
        string sourceText,
        IReadOnlyList<AiConversationMessage> conversation,
        CancellationToken cancellationToken = default);

    Task<AiResult<CvAiConversationResponse>> ContinueConversationAsync(
        CvAiProfile profile,
        string sourceText,
        IReadOnlyList<AiConversationMessage> history,
        string userMessage,
        CancellationToken cancellationToken = default);
}
