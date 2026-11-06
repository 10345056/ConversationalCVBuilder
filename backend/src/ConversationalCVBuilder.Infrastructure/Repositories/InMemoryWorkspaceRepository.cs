using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Infrastructure.Repositories;

public sealed class InMemoryWorkspaceRepository : IWorkspaceRepository
{
    private readonly Workspace _workspace = new();

    public Workspace Get() => _workspace;
}
