using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Domain.Entities;
using ConversationalCVBuilder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConversationalCVBuilder.Infrastructure.Repositories;

public sealed class ConversationRepository(ApplicationDbContext dbContext) : IConversationRepository
{
    public Task<ConversationSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ConversationSessions
            .AsSplitQuery()
            .Include(session => session.Messages.OrderBy(message => message.Sequence))
            .Include(session => session.QuestionStates.OrderBy(question => question.Order))
            .Include(session => session.CurrentQuestionState)
            .SingleOrDefaultAsync(session => session.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationSession>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ConversationSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.StartedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ConversationSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        await dbContext.ConversationSessions.AddAsync(session, cancellationToken);
    }

    public void Update(ConversationSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var entry = dbContext.Entry(session);
        if (entry.State == EntityState.Detached)
        {
            dbContext.ConversationSessions.Attach(session);
            entry.State = EntityState.Modified;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
