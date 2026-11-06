using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Common.Exceptions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Domain.Common;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.GenerateCv;

public sealed class GenerateCvCommandHandler(
    IWorkspaceRepository repository,
    ICvProfileMapper mapper) : IRequestHandler<GenerateCvCommand, GeneratedCvDto>
{
    public Task<GeneratedCvDto> Handle(GenerateCvCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = repository.Get()
            ?? throw new NotFoundException("Workspace", "current");
        if (workspace.Profile.Id != request.CvProfileId)
        {
            throw new NotFoundException("CV profile", request.CvProfileId);
        }

        if (string.IsNullOrWhiteSpace(workspace.Profile.FullName))
        {
            throw new DomainException("Add a name to the CV profile before generating the CV.");
        }

        workspace.Profile.MarkCompleted();
        return Task.FromResult(new GeneratedCvDto(mapper.Map(workspace.Profile), DateTimeOffset.UtcNow));
    }
}
