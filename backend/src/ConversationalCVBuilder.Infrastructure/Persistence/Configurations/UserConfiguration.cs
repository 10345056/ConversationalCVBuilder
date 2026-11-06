using ConversationalCVBuilder.Domain.Entities;
using ConversationalCVBuilder.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ConversationalCVBuilder.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Users");
        builder.Property(user => user.Email)
            .HasConversion(new ValueConverter<EmailAddress, string>(
                email => email.Value,
                value => EmailAddress.Create(value)))
            .UseCollation("NOCASE")
            .HasMaxLength(320)
            .IsRequired();
        builder.Ignore(user => user.NormalizedEmail);
        builder.Property(user => user.DisplayName).HasMaxLength(160).IsRequired();
        builder.Property(user => user.ExternalIdentityProvider).HasMaxLength(80);
        builder.Property(user => user.ExternalSubject).HasMaxLength(200);
        builder.Property(user => user.EmailVerified).IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.LastSeenAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));

        builder.HasMany(user => user.CvProfiles)
            .WithOne(profile => profile.Owner)
            .HasForeignKey(profile => profile.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(user => user.CvProfiles)
            .HasField("_cvProfiles").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(user => user.ConversationSessions)
            .WithOne(session => session.User)
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(user => user.ConversationSessions)
            .HasField("_conversationSessions").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => new { user.ExternalIdentityProvider, user.ExternalSubject })
            .IsUnique()
            .HasFilter("\"ExternalIdentityProvider\" IS NOT NULL AND \"ExternalSubject\" IS NOT NULL");
    }
}
