using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinhKhanhGuide.Domain.Entities;

namespace VinhKhanhGuide.Infrastructure.Persistence.Configurations;

public class StallConfiguration : IEntityTypeConfiguration<Stall>
{
    public void Configure(EntityTypeBuilder<Stall> builder)
    {
        builder.ToTable("Stalls");

        builder.HasKey(stall => stall.Id);

        builder.Property(stall => stall.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(stall => stall.DescriptionVi)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(stall => stall.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(stall => stall.OpenHours)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(stall => stall.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(stall => stall.ImageUrlsJson)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(stall => stall.Address)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(stall => stall.Phone)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(stall => stall.ReviewsJson)
            .HasMaxLength(8000)
            .IsRequired();

        builder.Property(stall => stall.MapLink)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(stall => stall.AudioUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(stall => stall.NarrationScriptVi)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(stall => stall.TriggerRadiusMeters)
            .HasPrecision(8, 2);

        builder.Property(stall => stall.AverageRating)
            .HasPrecision(3, 2);

        builder.Property(stall => stall.IsActive)
            .HasDefaultValue(true);
    }
}
