using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Services;
using ConversationalCVBuilder.Domain.Entities;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Templates.Commands.SelectTemplate;

public sealed class SelectTemplateCommandHandler(WorkspaceService workspaceService)
    : IRequestHandler<SelectTemplateCommand, Workspace>
{
    public Task<Workspace> Handle(
        SelectTemplateCommand request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(workspaceService.SelectTemplate(request.TemplateKey));
    }
}
