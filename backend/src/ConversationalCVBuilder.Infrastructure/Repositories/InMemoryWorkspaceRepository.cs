using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Infrastructure.Repositories;

public sealed class InMemoryWorkspaceRepository : IWorkspaceRepository
{
    private Workspace _workspace = new();

    public Workspace Get() => Volatile.Read(ref _workspace);

    public Workspace StartNewSession() =>
        Interlocked.Exchange(ref _workspace, new Workspace());
}
