using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Common.Exceptions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Domain.ValueObjects;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.UpdateCv;

public sealed class UpdateCvCommandHandler(
    IWorkspaceRepository repository,
    ICvProfileMapper mapper) : IRequestHandler<UpdateCvCommand, CvProfileDto>
{
    public Task<CvProfileDto> Handle(UpdateCvCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = repository.Get()
            ?? throw new NotFoundException("Workspace", "current");
        var profile = workspace.Profile;
        if (profile.Id != request.CvProfileId)
        {
            throw new NotFoundException("CV profile", request.CvProfileId);
        }

        if (request.Title is not null)
        {
            profile.UpdateTitle(request.Title);
        }

        if (request.TargetRole is not null)
        {
            profile.UpdateTargetRole(request.TargetRole);
        }

        if (request.Summary is not null)
        {
            profile.UpdateSummary(request.Summary);
        }

        if (request.Skills is not null)
        {
            profile.ReplaceSkills(request.Skills);
        }

        var personal = profile.PersonalInformation;
        if (request.GivenName is not null
            || request.FamilyName is not null
            || request.Email is not null
            || request.Phone is not null
            || request.Location is not null)
        {
            profile.UpdatePersonalInformation(
                request.GivenName ?? personal.GivenName,
                request.FamilyName ?? personal.FamilyName,
                request.Email is null
                    ? personal.Email
                    : string.IsNullOrWhiteSpace(request.Email) ? null : EmailAddress.Create(request.Email),
                request.Phone is null
                    ? personal.Phone
                    : string.IsNullOrWhiteSpace(request.Phone) ? null : PhoneNumber.Create(request.Phone),
                request.Location is null
                    ? personal.Address
                    : string.IsNullOrWhiteSpace(request.Location) ? null : PostalAddress.Create(request.Location));
        }

        return Task.FromResult(mapper.Map(profile));
    }
}
