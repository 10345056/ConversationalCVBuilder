using ConversationalCVBuilder.Application.Common.Exceptions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Abstractions;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Chat.Queries.GetConversation;

public sealed class GetConversationQueryHandler(IWorkspaceRepository repository)
    : IRequestHandler<GetConversationQuery, ConversationDto>
{
    public Task<ConversationDto> Handle(
        GetConversationQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = repository.Get()
            ?? throw new NotFoundException("Workspace", "current");
        var conversation = workspace.Conversation;
        if (request.SessionId.HasValue && conversation.Id != request.SessionId.Value)
        {
            throw new NotFoundException("Conversation session", request.SessionId.Value);
        }

        var result = new ConversationDto(
            conversation.Id,
            conversation.Status.ToString(),
            workspace.CurrentStep,
            conversation.Messages
                .OrderBy(message => message.Sequence)
                .Select(message => new ConversationMessageDto(
                    message.Id,
                    message.Role.ToString(),
                    message.Type.ToString(),
                    message.Content,
                    message.CreatedAtUtc))
                .ToArray(),
            conversation.QuestionStates
                .OrderBy(question => question.Order)
                .Select(question => new QuestionStateDto(
                    question.Id,
                    question.Key,
                    question.Prompt,
                    question.Order,
                    question.Status.ToString(),
                    question.Answer))
                .ToArray());
        return Task.FromResult(result);
    }
}
