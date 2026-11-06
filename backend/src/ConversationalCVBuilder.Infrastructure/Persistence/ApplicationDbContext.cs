using ConversationalCVBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConversationalCVBuilder.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<CvProfile> CvProfiles => Set<CvProfile>();
    public DbSet<PersonalInformation> PersonalInformation => Set<PersonalInformation>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Education> EducationEntries => Set<Education>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ConversationSession> ConversationSessions => Set<ConversationSession>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<QuestionState> QuestionStates => Set<QuestionState>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

}
