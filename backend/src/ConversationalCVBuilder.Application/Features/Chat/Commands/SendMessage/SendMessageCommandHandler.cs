using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Services;
using ConversationalCVBuilder.Domain.Entities;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Chat.Commands.SendMessage;

public sealed class SendMessageCommandHandler(
    ConversationService conversationService,
    IAiService aiService,
    IWorkspaceRepository workspaceRepository,
    WorkspaceService workspaceService)
    : IRequestHandler<SendMessageCommand, Workspace>
{
    public async Task<Workspace> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = workspaceRepository.Get();
        var history = workspace.Messages
            .Select(message => new AiConversationMessage(message.Role, message.Content))
            .ToArray();
        var result = await aiService.ContinueConversationAsync(
            workspaceService.GetAiProfile(),
            workspace.SourceText,
            history,
            request.Message,
            cancellationToken);

        if (result.Value is { } response && !string.IsNullOrWhiteSpace(response.Profile.FullName))
        {
            workspaceService.UpdateProfile(response.Profile.WithFallback(workspaceService.GetAiProfile()));
            workspace = workspaceRepository.Get();
            workspace.AddMessage("user", request.Message.Trim());
            workspace.AddMessage("assistant", response.Reply);
            workspace.SetCurrentStep(response.MissingFields.Count == 0 ? "complete" : "ai-follow-up");
            workspaceService.SetAiInsights(
                "success",
                null,
                response.MissingFields,
                response.NextQuestion,
                response.SuggestedImprovements,
                response.TemplateRecommendation);
            workspaceService.SelectTemplate(response.TemplateRecommendation);
            return workspace;
        }

        var error = result.Error
            ?? "Your update could not be confirmed. Please review your CV details and try again.";
        workspaceService.SetAiInsights("fallback", error);
        return conversationService.Send(request.Message, error);
    }
}
