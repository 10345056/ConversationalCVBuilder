using ConversationalCVBuilder.Domain.Entities;
using ConversationalCVBuilder.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ConversationalCVBuilder.Infrastructure.Persistence.Configurations;

public sealed class CvProfileConfiguration : IEntityTypeConfiguration<CvProfile>
{
    public void Configure(EntityTypeBuilder<CvProfile> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("CvProfiles");
        builder.Property(profile => profile.OwnerId).IsRequired();
        builder.Property(profile => profile.Title).HasField("_title").UsePropertyAccessMode(PropertyAccessMode.Field).HasMaxLength(120).IsRequired();
        builder.Property(profile => profile.TargetRole).HasField("_targetRole").UsePropertyAccessMode(PropertyAccessMode.Field).HasMaxLength(160).IsRequired();
        builder.Property(profile => profile.Summary).HasField("_summary").UsePropertyAccessMode(PropertyAccessMode.Field).HasMaxLength(5000).IsRequired();
        builder.Property<string>("_additionalInformation")
            .HasColumnName("AdditionalInformation")
            .HasMaxLength(5000)
            .IsRequired();
        builder.Property(profile => profile.CompletedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.Property(profile => profile.LastUsedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.Property(profile => profile.IsDefault).IsRequired();
        builder.Property(profile => profile.SelectedTemplateKey).HasMaxLength(80).IsRequired();
        builder.Property(profile => profile.SelectedTemplateId);
        builder.Ignore(profile => profile.DomainEvents);
        builder.Ignore(profile => profile.FullName);
        builder.Ignore(profile => profile.Email);
        builder.Ignore(profile => profile.Phone);
        builder.Ignore(profile => profile.Location);
        builder.Ignore(profile => profile.Skills);
        builder.Ignore(profile => profile.Experience);
        builder.Ignore(profile => profile.Education);
        builder.Ignore(profile => profile.CertificationLabels);
        builder.Ignore(profile => profile.ProjectSummaries);
        builder.Ignore(profile => profile.LanguageLabels);
        builder.Ignore(profile => profile.Achievements);
        builder.Ignore(profile => profile.AdditionalInformation);

        builder.HasOne(profile => profile.Owner)
            .WithMany(user => user.CvProfiles)
            .HasForeignKey(profile => profile.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(profile => profile.SelectedTemplate)
            .WithMany()
            .HasForeignKey(profile => profile.SelectedTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(profile => profile.PersonalInformation)
            .WithOne(personal => personal.CvProfile)
            .HasForeignKey<PersonalInformation>(personal => personal.CvProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(profile => profile.WorkExperiences)
            .WithOne(experience => experience.CvProfile)
            .HasForeignKey(experience => experience.CvProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(profile => profile.WorkExperiences)
            .HasField("_workExperiences").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(profile => profile.EducationEntries)
            .WithOne(education => education.CvProfile)
            .HasForeignKey(education => education.CvProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(profile => profile.EducationEntries)
            .HasField("_education").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(profile => profile.Certifications)
            .WithOne(certification => certification.CvProfile)
            .HasForeignKey(certification => certification.CvProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(profile => profile.Certifications)
            .HasField("_certifications").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(profile => profile.SkillItems)
            .WithOne(skill => skill.CvProfile)
            .HasForeignKey(skill => skill.CvProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(profile => profile.SkillItems)
            .HasField("_skills").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(profile => profile.Languages)
            .WithOne(language => language.CvProfile)
            .HasForeignKey(language => language.CvProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(profile => profile.Languages)
            .HasField("_languages").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(profile => profile.Projects)
            .WithOne(project => project.CvProfile)
            .HasForeignKey(project => project.CvProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(profile => profile.Projects)
            .HasField("_projects").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(profile => new { profile.OwnerId, profile.IsDefault })
            .IsUnique()
            .HasFilter("\"IsDefault\" = 1")
            .HasDatabaseName("IX_CvProfiles_Owner_Default");
        builder.HasIndex(profile => profile.SelectedTemplateKey);

    }
}

public sealed class PersonalInformationConfiguration : IEntityTypeConfiguration<PersonalInformation>
{
    public void Configure(EntityTypeBuilder<PersonalInformation> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("PersonalInformation");
        builder.Property(personal => personal.CvProfileId).IsRequired();
        builder.Property(personal => personal.GivenName).HasMaxLength(100).IsRequired();
        builder.Property(personal => personal.FamilyName).HasMaxLength(100).IsRequired();
        builder.Ignore(personal => personal.FullName);
        builder.Property(personal => personal.ProfessionalHeadline).HasMaxLength(200);
        builder.Property(personal => personal.Email)
            .HasConversion(new ValueConverter<EmailAddress?, string?>(
                value => value == null ? null : value.Value,
                value => value == null ? null : EmailAddress.Create(value)))
            .HasMaxLength(320);
        builder.Property(personal => personal.Phone)
            .HasConversion(new ValueConverter<PhoneNumber?, string?>(
                value => value == null ? null : value.Value,
                value => value == null ? null : PhoneNumber.Create(value)))
            .HasMaxLength(40);
        builder.Property(personal => personal.Address)
            .HasConversion(new ValueConverter<PostalAddress?, string?>(
                value => value == null ? null : value.Formatted,
                value => value == null ? null : PostalAddress.Create(value)))
            .HasMaxLength(300);
        builder.Property(personal => personal.Website)
            .HasConversion(new ValueConverter<Uri?, string?>(
                value => value == null ? null : value.ToString(),
                value => value == null ? null : new Uri(value, UriKind.Absolute)))
            .HasMaxLength(2048);
        builder.Property(personal => personal.LinkedInUrl)
            .HasConversion(new ValueConverter<Uri?, string?>(
                value => value == null ? null : value.ToString(),
                value => value == null ? null : new Uri(value, UriKind.Absolute)))
            .HasMaxLength(2048);
        builder.HasIndex(personal => personal.CvProfileId).IsUnique();
    }
}

public sealed class WorkExperienceConfiguration : IEntityTypeConfiguration<WorkExperience>
{
    public void Configure(EntityTypeBuilder<WorkExperience> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("WorkExperiences");
        builder.Property(item => item.PositionTitle).HasMaxLength(160).IsRequired();
        builder.Property(item => item.EmployerName).HasMaxLength(160);
        builder.Property(item => item.Location).HasMaxLength(160);
        builder.Property(item => item.Description).HasMaxLength(5000);
        builder.Property(item => item.EmploymentType).HasConversion<int?>();
        builder.Property(item => item.DisplayOrder).IsRequired();
        builder.HasIndex(item => new { item.CvProfileId, item.DisplayOrder });
    }
}

public sealed class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Education");
        builder.Property(item => item.Qualification).HasMaxLength(160).IsRequired();
        builder.Property(item => item.InstitutionName).HasMaxLength(160);
        builder.Property(item => item.FieldOfStudy).HasMaxLength(160);
        builder.Property(item => item.Description).HasMaxLength(3000);
        builder.Property(item => item.GradeScale).HasMaxLength(40);
        builder.Property(item => item.Grade).HasPrecision(5, 2);
        builder.HasIndex(item => new { item.CvProfileId, item.DisplayOrder });
    }
}

public sealed class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Certifications");
        builder.Property(item => item.Name).HasMaxLength(160).IsRequired();
        builder.Property(item => item.IssuingOrganization).HasMaxLength(160);
        builder.Property(item => item.CredentialId).HasMaxLength(120);
        builder.Property(item => item.CredentialUrl)
            .HasConversion(value => value == null ? null : value.ToString(), value => value == null ? null : new Uri(value, UriKind.Absolute))
            .HasMaxLength(2048);
        builder.HasIndex(item => new { item.CvProfileId, item.DisplayOrder });
    }
}

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Skills");
        builder.Property(item => item.Name).UseCollation("NOCASE").HasMaxLength(100).IsRequired();
        builder.Property(item => item.Category).HasMaxLength(100);
        builder.Property(item => item.Proficiency).HasConversion<int?>();
        builder.HasIndex(item => new { item.CvProfileId, item.Name }).IsUnique();
        builder.HasIndex(item => new { item.CvProfileId, item.DisplayOrder });
    }
}

public sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Languages");
        builder.Property(item => item.Name).UseCollation("NOCASE").HasMaxLength(100).IsRequired();
        builder.Property(item => item.Proficiency).HasConversion<int>().IsRequired();
        builder.HasIndex(item => new { item.CvProfileId, item.Name }).IsUnique();
        builder.HasIndex(item => new { item.CvProfileId, item.DisplayOrder });
    }
}
