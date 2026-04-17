using Microsoft.EntityFrameworkCore;
using VinhKhanhGuide.Domain.Entities;
using VinhKhanhGuide.Infrastructure.Persistence.Configurations;

namespace VinhKhanhGuide.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUsageSession> AppUsageSessions => Set<AppUsageSession>();

    public DbSet<Stall> Stalls => Set<Stall>();
    public DbSet<StallTranslation> StallTranslations => Set<StallTranslation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppUsageSessionConfiguration());
        modelBuilder.ApplyConfiguration(new StallConfiguration());
        modelBuilder.ApplyConfiguration(new StallTranslationConfiguration());
    }
}
