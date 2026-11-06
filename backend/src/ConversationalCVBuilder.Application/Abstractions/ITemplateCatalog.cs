using ConversationalCVBuilder.Application.Contracts;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface ITemplateCatalog
{
    IReadOnlyList<TemplateDto> GetAll();
}
