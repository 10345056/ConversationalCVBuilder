using ConversationalCVBuilder.Domain.Entities;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Templates.Commands.SelectTemplate;

public sealed record SelectTemplateCommand(string TemplateKey) : IRequest<Workspace>;
