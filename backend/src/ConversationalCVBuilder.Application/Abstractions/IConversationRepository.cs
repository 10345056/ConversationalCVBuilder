using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface IConversationRepository
{
    Task<ConversationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConversationSession>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task AddAsync(ConversationSession session, CancellationToken cancellationToken = default);
    void Update(ConversationSession session);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
