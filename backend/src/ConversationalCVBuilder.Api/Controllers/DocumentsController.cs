using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Features.Documents.Commands.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController(
    ISender sender) : ControllerBase
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    [HttpPost("parse")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [ProducesResponseType<UploadedDocument>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadedDocument>> Parse(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Choose a non-empty PDF, DOCX, or PPTX file." });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { error = "Files must be 10 MB or smaller." });
        }

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        var result = await sender.Send(
            new UploadDocumentCommand(file.FileName, stream.ToArray(), file.ContentType),
            cancellationToken);
        return Ok(result);
    }
}
