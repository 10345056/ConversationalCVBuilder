using System.Text.Json;
using ConversationalCVBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConversationalCVBuilder.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    private static readonly ValueComparer<List<string>> TechnologiesComparer = new(
        (left, right) => left != null && right != null && left.SequenceEqual(right),
        value => value == null
            ? 0
            : value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
        value => value == null ? new List<string>() : value.ToList());

    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Projects");
        builder.Property(item => item.CvProfileId).IsRequired();
        builder.Property(item => item.Name).HasMaxLength(160).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(5000);
        builder.Property(item => item.ProjectUrl)
            .HasConversion(value => value == null ? null : value.ToString(), value => value == null ? null : new Uri(value, UriKind.Absolute))
            .HasMaxLength(2048);
        builder.Property<List<string>>("_technologies")
            .HasColumnName("TechnologiesJson")
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(TechnologiesComparer);
        builder.Property(item => item.DisplayOrder).IsRequired();
        builder.HasIndex(item => new { item.CvProfileId, item.DisplayOrder });
    }
}
