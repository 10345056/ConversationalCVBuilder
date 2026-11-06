using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Common.Exceptions;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.CreateCv;

public sealed class CreateCvCommandHandler(
    IWorkspaceRepository repository,
    ICvProfileMapper mapper) : IRequestHandler<CreateCvCommand, CvProfileDto>
{
    public Task<CvProfileDto> Handle(CreateCvCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = repository.Get()
            ?? throw new NotFoundException("Workspace", "current");
        var profile = workspace.CreateProfile(request.Title);
        return Task.FromResult(mapper.Map(profile));
    }
}
