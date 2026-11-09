using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface IWorkspaceRepository
{
    Workspace Get();
    Workspace StartNewSession();
}
