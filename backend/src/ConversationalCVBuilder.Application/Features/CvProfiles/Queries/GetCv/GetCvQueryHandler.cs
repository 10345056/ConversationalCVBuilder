using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Common.Exceptions;
using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Queries.GetCv;

public sealed class GetCvQueryHandler(
    IWorkspaceRepository repository,
    ICvProfileMapper mapper) : IRequestHandler<GetCvQuery, CvProfileDto>
{
    public Task<CvProfileDto> Handle(GetCvQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = repository.Get()
            ?? throw new NotFoundException("Workspace", "current");
        if (request.CvProfileId.HasValue && workspace.Profile.Id != request.CvProfileId.Value)
        {
            throw new NotFoundException("CV profile", request.CvProfileId.Value);
        }

        return Task.FromResult(mapper.Map(workspace.Profile));
    }
}
