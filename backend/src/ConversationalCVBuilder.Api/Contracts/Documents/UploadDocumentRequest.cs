using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ConversationalCVBuilder.Api.Contracts.Documents;

public sealed class UploadDocumentRequest
{
    [Required]
    public IFormFile? File { get; init; }
}
