using ConversationalCVBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConversationalCVBuilder.Infrastructure.Persistence.Configurations;

public sealed class ConversationSessionConfiguration : IEntityTypeConfiguration<ConversationSession>
{
    public void Configure(EntityTypeBuilder<ConversationSession> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("ConversationSessions");
        builder.Property(session => session.UserId).IsRequired();
        builder.Property(session => session.CvProfileId);
        builder.Property(session => session.Locale).HasMaxLength(35).IsRequired();
        builder.Property(session => session.Status).HasConversion<int>().IsRequired();
        builder.Property(session => session.StartedAtUtc)
            .HasConversion(
                value => value.ToString("O"),
                value => DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture))
            .IsRequired();
        builder.Property(session => session.CompletedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.Property(session => session.LastActivityAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.Property(session => session.CurrentQuestionStateId);

        builder.HasOne(session => session.CvProfile)
            .WithMany()
            .HasForeignKey(session => session.CvProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(session => session.Messages)
            .WithOne(message => message.Session)
            .HasForeignKey(message => message.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(session => session.Messages)
            .HasField("_messages").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(session => session.QuestionStates)
            .WithOne(question => question.Session)
            .HasForeignKey(question => question.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(session => session.QuestionStates)
            .HasField("_questionStates").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(session => session.CurrentQuestionState)
            .WithMany()
            .HasForeignKey(session => session.CurrentQuestionStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(session => new { session.UserId, session.StartedAtUtc });
        builder.HasIndex(session => new { session.UserId, session.Status });
    }
}

public sealed class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("ConversationMessages");
        builder.Property(message => message.SessionId).IsRequired();
        builder.Property(message => message.Role).HasConversion<int>().IsRequired();
        builder.Property(message => message.Type).HasConversion<int>().IsRequired();
        builder.Property(message => message.Content).HasMaxLength(20_000).IsRequired();
        builder.Property(message => message.Sequence).IsRequired();
        builder.Property(message => message.AudioContentType).HasMaxLength(128);
        builder.Property(message => message.CreatedAtUtc)
            .HasConversion(
                value => value.ToString("O"),
                value => DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture))
            .IsRequired();
        builder.HasIndex(message => new { message.SessionId, message.Sequence }).IsUnique();
    }
}

public sealed class QuestionStateConfiguration : IEntityTypeConfiguration<QuestionState>
{
    public void Configure(EntityTypeBuilder<QuestionState> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("QuestionStates");
        builder.Property(question => question.SessionId).IsRequired();
        builder.Property(question => question.Key).HasMaxLength(100).IsRequired();
        builder.Property(question => question.Prompt).HasMaxLength(2000).IsRequired();
        builder.Property(question => question.Order).IsRequired();
        builder.Property(question => question.Status).HasConversion<int>().IsRequired();
        builder.Property(question => question.Answer).HasMaxLength(10_000);
        builder.Property(question => question.AskedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.Property(question => question.AnsweredAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.HasIndex(question => new { question.SessionId, question.Key }).IsUnique();
        builder.HasIndex(question => new { question.SessionId, question.Order }).IsUnique();
    }
}
