using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Templates.Queries.GetTemplates;

public sealed record GetTemplatesQuery : IRequest<IReadOnlyList<TemplateDto>>;
