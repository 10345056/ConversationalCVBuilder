using ConversationalCVBuilder.Api.Contracts.Cv;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Features.CvProfiles.Commands.GenerateCv;
using ConversationalCVBuilder.Application.Features.CvProfiles.Queries.GetCv;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/cv")]
public sealed class CvController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CvProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CvProfileDto>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var profile = await sender.Send(new GetCvQuery(id), cancellationToken);
        return Ok(profile);
    }

    [HttpPost("generate")]
    [ProducesResponseType<GeneratedCvDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeneratedCvDto>> Generate(
        [FromBody] GenerateCvRequest request,
        CancellationToken cancellationToken)
    {
        var generated = await sender.Send(
            new GenerateCvCommand(request.CvProfileId),
            cancellationToken);
        return Ok(generated);
    }
}
