using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.GenerateCv;

public sealed record GenerateCvCommand(Guid CvProfileId) : IRequest<GeneratedCvDto>;
