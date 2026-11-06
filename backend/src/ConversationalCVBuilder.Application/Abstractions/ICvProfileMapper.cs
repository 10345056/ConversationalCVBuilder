using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface ICvProfileMapper
{
    CvProfileDto Map(CvProfile profile);
}
