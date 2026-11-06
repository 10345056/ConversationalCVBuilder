using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Features.Templates.Queries.GetTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/templates")]
public sealed class TemplatesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TemplateDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TemplateDto>>> Get(
        CancellationToken cancellationToken)
    {
        var templates = await sender.Send(new GetTemplatesQuery(), cancellationToken);
        return Ok(templates);
    }
}
