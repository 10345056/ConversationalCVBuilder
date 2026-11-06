using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Chat.Queries.GetConversation;

public sealed record GetConversationQuery(Guid? SessionId = null) : IRequest<ConversationDto>;
