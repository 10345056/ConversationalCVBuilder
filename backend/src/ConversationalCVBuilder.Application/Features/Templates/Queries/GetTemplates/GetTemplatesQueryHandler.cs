using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Templates.Queries.GetTemplates;

public sealed class GetTemplatesQueryHandler(ITemplateCatalog templateCatalog)
    : IRequestHandler<GetTemplatesQuery, IReadOnlyList<TemplateDto>>
{
    public Task<IReadOnlyList<TemplateDto>> Handle(
        GetTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(templateCatalog.GetAll());
    }
}
