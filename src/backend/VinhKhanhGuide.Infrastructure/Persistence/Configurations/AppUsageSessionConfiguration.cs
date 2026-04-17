using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinhKhanhGuide.Domain.Entities;

namespace VinhKhanhGuide.Infrastructure.Persistence.Configurations;

public sealed class AppUsageSessionConfiguration : IEntityTypeConfiguration<AppUsageSession>
{
    public void Configure(EntityTypeBuilder<AppUsageSession> builder)
    {
        builder.ToTable("AppUsageSessions");

        builder.HasKey(session => session.SessionId);

        builder.Property(session => session.SessionId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(session => session.AnonymousClientId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(session => session.LastEventType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(session => session.AppVersion)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(session => session.Platform)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(session => session.LastSeenAtUtc);
        builder.HasIndex(session => session.AnonymousClientId);
    }
}
