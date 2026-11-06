using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface IDocxExporter
{
    byte[] Export(CvProfile profile, string templateId);
}
