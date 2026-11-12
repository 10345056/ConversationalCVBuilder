using System.Text.Json;
using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Common.Exceptions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Services;
using MediatR;

namespace ConversationalCVBuilder.Application.Features.Exports.Commands.ExportCv;

public sealed class ExportCvCommandHandler(
    IWorkspaceRepository repository,
    ICvProfileMapper mapper,
    IDocxExporter docxExporter,
    IPdfExporter pdfExporter) : IRequestHandler<ExportCvCommand, ExportedCvFile>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public Task<ExportedCvFile> Handle(
        ExportCvCommand request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = repository.Get()
            ?? throw new NotFoundException("Workspace", "current");
        if (request.CvProfileId.HasValue && workspace.Profile.Id != request.CvProfileId.Value)
        {
            throw new NotFoundException("CV profile", request.CvProfileId.Value);
        }

        var format = request.Format.Trim().ToLowerInvariant();
        var profile = workspace.Profile;
        var templateKey = request.TemplateKey ?? profile.SelectedTemplateKey;
        var (content, contentType, extension) = format switch
        {
            "docx" => (
                docxExporter.Export(profile, templateKey),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "docx"),
            "pdf" => (
                pdfExporter.Export(profile, templateKey, request.DesignConfiguration),
                "application/pdf",
                "pdf"),
            "json" => (
                JsonSerializer.SerializeToUtf8Bytes(mapper.Map(profile), JsonOptions),
                "application/json",
                "json"),
            _ => throw new InvalidOperationException($"Export format '{format}' is not supported.")
        };

        return Task.FromResult(new ExportedCvFile(
            $"{CreateFileName(profile.FullName)}.{extension}",
            contentType,
            content));
    }

    private static string CreateFileName(string fullName)
    {
        var characters = fullName
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();
        var normalized = new string(characters);
        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        return normalized.Trim('-') is { Length: > 0 } fileName ? fileName : "cv";
    }
}
