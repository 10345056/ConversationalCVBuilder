using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Features.Documents.Commands.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController(
    ISender sender,
    ILogger<DocumentsController> logger) : ControllerBase
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
            return BadRequest(new { error = "Choose a non-empty PDF, DOCX, PPTX, or TXT file." });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { error = "Files must be 10 MB or smaller." });
        }

        logger.LogInformation(
            "Received resume upload {FileName}: {FileSizeBytes} bytes, {ContentType}",
            Path.GetFileName(file.FileName),
            file.Length,
            file.ContentType);
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        var result = await sender.Send(
            new UploadDocumentCommand(file.FileName, stream.ToArray(), file.ContentType),
            cancellationToken);
        if (result.Status == "partial")
        {
            logger.LogWarning(
                "Resume extraction was partial for {FileName}: {FileSizeBytes} bytes, {ContentType}, {CharacterCount} characters. {Diagnostic}",
                result.FileName,
                result.FileSizeBytes,
                result.ContentType,
                result.ExtractedCharacterCount,
                result.AiError ?? result.Message);
        }
        else
        {
            logger.LogInformation(
                "Resume extraction succeeded for {FileName}: {FileSizeBytes} bytes, {ContentType}, {CharacterCount} characters",
                result.FileName,
                result.FileSizeBytes,
                result.ContentType,
                result.ExtractedCharacterCount);
        }

        return Ok(result);
    }
}
