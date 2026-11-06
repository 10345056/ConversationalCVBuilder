using ConversationalCVBuilder.Api.Contracts.Exports;
using ConversationalCVBuilder.Application.Features.Exports.Commands.ExportCv;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/export")]
public sealed class ExportController(ISender sender) : ControllerBase
{
    [HttpGet("docx")]
    [Produces("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportDocx(
        [FromQuery] string? templateId,
        CancellationToken cancellationToken)
    {
        var export = await sender.Send(
            new ExportCvCommand("docx", TemplateKey: templateId),
            cancellationToken);
        return File(export.Content, export.ContentType, export.FileName);
    }

    [HttpPost("docx")]
    [Consumes("application/json")]
    [Produces("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportDocx(
        [FromBody] ExportCvRequest request,
        CancellationToken cancellationToken)
    {
        var export = await sender.Send(
            new ExportCvCommand("docx", request.CvProfileId, request.TemplateKey),
            cancellationToken);
        return File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportJson(CancellationToken cancellationToken)
    {
        var export = await sender.Send(new ExportCvCommand("json"), cancellationToken);
        return File(export.Content, export.ContentType, export.FileName);
    }

    [HttpPost("pdf")]
    [Consumes("application/json")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportPdf(
        [FromBody] ExportCvRequest request,
        CancellationToken cancellationToken)
    {
        var export = await sender.Send(
            new ExportCvCommand("pdf", request.CvProfileId, request.TemplateKey),
            cancellationToken);
        return File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPdf(CancellationToken cancellationToken)
    {
        var export = await sender.Send(new ExportCvCommand("pdf"), cancellationToken);
        return File(export.Content, export.ContentType, export.FileName);
    }
}
