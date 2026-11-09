using ConversationalCVBuilder.Api.Contracts.Documents;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Features.Documents.Commands.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/upload")]
public sealed class UploadController(
    ISender sender,
    ILogger<UploadController> logger) : ControllerBase
{
    private const long MaximumFileSizeBytes = 10 * 1024 * 1024;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumFileSizeBytes)]
    [ProducesResponseType<UploadedDocument>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadedDocument>> Upload(
        [FromForm] UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(nameof(request.File), "Choose a non-empty PDF, DOCX, PPTX, or TXT file.");
            return ValidationProblem(ModelState);
        }

        if (file.Length > MaximumFileSizeBytes)
        {
            ModelState.AddModelError(nameof(request.File), "Files must be 10 MB or smaller.");
            return ValidationProblem(ModelState);
        }

        logger.LogInformation(
            "Received resume upload {FileName}: {FileSizeBytes} bytes, {ContentType}",
            Path.GetFileName(file.FileName),
            file.Length,
            file.ContentType);
        await using var content = new MemoryStream((int)file.Length);
        await file.CopyToAsync(content, cancellationToken);
        var result = await sender.Send(
            new UploadDocumentCommand(file.FileName, content.ToArray(), file.ContentType),
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
