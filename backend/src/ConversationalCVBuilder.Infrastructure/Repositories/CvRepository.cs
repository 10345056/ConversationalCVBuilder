using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Domain.Entities;
using ConversationalCVBuilder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConversationalCVBuilder.Infrastructure.Repositories;

public sealed class CvRepository(ApplicationDbContext dbContext) : ICvRepository
{
    public Task<CvProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CvProfiles
            .AsSplitQuery()
            .Include(profile => profile.PersonalInformation)
            .Include(profile => profile.WorkExperiences)
            .Include(profile => profile.EducationEntries)
            .Include(profile => profile.Certifications)
            .Include(profile => profile.SkillItems)
            .Include(profile => profile.Languages)
            .Include(profile => profile.Projects)
            .Include(profile => profile.SelectedTemplate)
            .SingleOrDefaultAsync(profile => profile.Id == id, cancellationToken);
    }

    public async Task AddAsync(CvProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        await dbContext.CvProfiles.AddAsync(profile, cancellationToken);
    }

    public void Update(CvProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var entry = dbContext.Entry(profile);
        if (entry.State == EntityState.Detached)
        {
            dbContext.CvProfiles.Attach(profile);
            entry.State = EntityState.Modified;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
