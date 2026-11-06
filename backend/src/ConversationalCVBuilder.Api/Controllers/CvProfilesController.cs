using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Features.CvProfiles.Commands.CreateCv;
using ConversationalCVBuilder.Application.Features.CvProfiles.Commands.GenerateCv;
using ConversationalCVBuilder.Application.Features.CvProfiles.Commands.UpdateCv;
using ConversationalCVBuilder.Application.Features.CvProfiles.Queries.GetCv;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/cv-profiles")]
public sealed class CvProfilesController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CvProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CvProfileDto>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var profile = await sender.Send(new GetCvQuery(id), cancellationToken);
        return Ok(profile);
    }

    [HttpPost]
    [ProducesResponseType<CvProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CvProfileDto>> Create(
        [FromBody] CreateCvCommand command,
        CancellationToken cancellationToken)
    {
        var profile = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = profile.Id }, profile);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CvProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CvProfileDto>> Update(
        Guid id,
        [FromBody] UpdateCvRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCvCommand(
            id,
            request.Title,
            request.TargetRole,
            request.Summary,
            request.Skills,
            request.GivenName,
            request.FamilyName,
            request.Email,
            request.Phone,
            request.Location);
        var profile = await sender.Send(command, cancellationToken);
        return Ok(profile);
    }

    [HttpPost("{id:guid}/generate")]
    [ProducesResponseType<GeneratedCvDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeneratedCvDto>> Generate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var generated = await sender.Send(new GenerateCvCommand(id), cancellationToken);
        return Ok(generated);
    }
}

public sealed record UpdateCvRequest(
    string? Title = null,
    string? TargetRole = null,
    string? Summary = null,
    IReadOnlyList<string>? Skills = null,
    string? GivenName = null,
    string? FamilyName = null,
    string? Email = null,
    string? Phone = null,
    string? Location = null);
