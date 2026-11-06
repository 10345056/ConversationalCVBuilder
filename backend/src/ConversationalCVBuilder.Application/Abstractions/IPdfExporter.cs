using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface IPdfExporter
{
    byte[] Export(CvProfile profile, string? templateKey = null);
}
