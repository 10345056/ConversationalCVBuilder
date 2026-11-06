using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.CreateCv;

public sealed record CreateCvCommand(string Title) : IRequest<CvProfileDto>;
