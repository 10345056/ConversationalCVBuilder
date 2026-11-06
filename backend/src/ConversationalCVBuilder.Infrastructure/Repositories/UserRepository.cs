using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Domain.Entities;
using ConversationalCVBuilder.Domain.ValueObjects;
using ConversationalCVBuilder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConversationalCVBuilder.Infrastructure.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(user => user.CvProfiles)
            .Include(user => user.ConversationSessions)
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var address = EmailAddress.Create(email);
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.Email == address,
            cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var entry = dbContext.Entry(user);
        if (entry.State == EntityState.Detached)
        {
            dbContext.Users.Attach(user);
            entry.State = EntityState.Modified;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
