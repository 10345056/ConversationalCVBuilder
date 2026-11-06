using ConversationalCVBuilder.Domain.Entities;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Chat.Commands.SendMessage;

public sealed record SendMessageCommand(string Message) : IRequest<Workspace>;
