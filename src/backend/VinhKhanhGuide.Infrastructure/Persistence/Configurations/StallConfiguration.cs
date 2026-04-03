using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinhKhanhGuide.Domain.Entities;
using VinhKhanhGuide.Infrastructure.Persistence.Seeding;

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

        builder.Property(stall => stall.TriggerRadiusMeters)
            .HasPrecision(8, 2);

        builder.Property(stall => stall.AverageRating)
            .HasPrecision(3, 2);

        builder.HasData(StallSeedData.Stalls);
    }
}
