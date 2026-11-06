using ConversationalCVBuilder.Domain.Entities;

namespace ConversationalCVBuilder.Application.Abstractions;

public interface ICvRepository
{
    Task<CvProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(CvProfile profile, CancellationToken cancellationToken = default);
    void Update(CvProfile profile);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
