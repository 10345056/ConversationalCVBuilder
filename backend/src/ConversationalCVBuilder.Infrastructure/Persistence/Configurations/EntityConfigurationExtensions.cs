using ConversationalCVBuilder.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConversationalCVBuilder.Infrastructure.Persistence.Configurations;

internal static class EntityConfigurationExtensions
{
    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.CreatedAtUtc)
            .IsRequired()
            .HasConversion(
                value => value.ToString("O"),
                value => DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.Property(entity => entity.UpdatedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.Property(entity => entity.CreatedBy);
        builder.Property(entity => entity.UpdatedBy);
        builder.Property(entity => entity.IsDeleted).IsRequired();
        builder.Property(entity => entity.DeletedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.ToString("O") : null,
                value => value == null
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        builder.HasIndex(entity => entity.IsDeleted);
        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
