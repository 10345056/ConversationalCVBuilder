using ConversationalCVBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConversationalCVBuilder.Infrastructure.Persistence.Configurations;

public sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Templates");
        builder.Property(template => template.Key).UseCollation("NOCASE").HasMaxLength(80).IsRequired();
        builder.Property(template => template.Name).HasMaxLength(100).IsRequired();
        builder.Property(template => template.Description).HasMaxLength(1000).IsRequired();
        builder.Property(template => template.Category).HasMaxLength(80).IsRequired();
        builder.Property(template => template.AccentColor).HasMaxLength(7).IsRequired();
        builder.Property(template => template.ConfigurationJson).IsRequired();
        builder.Property(template => template.IsBuiltIn).IsRequired();
        builder.Property(template => template.IsActive).IsRequired();
        builder.Property(template => template.DisplayOrder).IsRequired();
        builder.HasIndex(template => template.Key).IsUnique();
        builder.HasIndex(template => new { template.IsActive, template.DisplayOrder });
    }
}
