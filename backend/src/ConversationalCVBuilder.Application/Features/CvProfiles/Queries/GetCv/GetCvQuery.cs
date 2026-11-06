using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Queries.GetCv;

public sealed record GetCvQuery(Guid? CvProfileId = null) : IRequest<CvProfileDto>;
