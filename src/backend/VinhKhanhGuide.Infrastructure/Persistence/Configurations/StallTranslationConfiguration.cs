using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinhKhanhGuide.Domain.Entities;

namespace VinhKhanhGuide.Infrastructure.Persistence.Configurations;

public class StallTranslationConfiguration : IEntityTypeConfiguration<StallTranslation>
{
    public void Configure(EntityTypeBuilder<StallTranslation> builder)
    {
        builder.ToTable("StallTranslations");

        builder.HasKey(translation => translation.Id);

        builder.Property(translation => translation.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(translation => translation.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(translation => translation.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(translation => new { translation.StallId, translation.LanguageCode })
            .IsUnique();
    }
}
