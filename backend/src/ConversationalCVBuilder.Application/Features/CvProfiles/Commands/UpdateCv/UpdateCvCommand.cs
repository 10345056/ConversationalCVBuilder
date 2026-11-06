using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.UpdateCv;

public sealed record UpdateCvCommand(
    Guid CvProfileId,
    string? Title = null,
    string? TargetRole = null,
    string? Summary = null,
    IReadOnlyList<string>? Skills = null,
    string? GivenName = null,
    string? FamilyName = null,
    string? Email = null,
    string? Phone = null,
    string? Location = null) : IRequest<CvProfileDto>;
