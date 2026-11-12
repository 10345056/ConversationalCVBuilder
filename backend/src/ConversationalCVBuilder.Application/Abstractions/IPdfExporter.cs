using ConversationalCVBuilder.Domain.Entities;
using ConversationalCVBuilder.Application.Contracts;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface IPdfExporter
{
    byte[] Export(
        CvProfile profile,
        string? templateKey = null,
        CvDesignConfiguration? designConfiguration = null);
}
