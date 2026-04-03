using Microsoft.EntityFrameworkCore;
using VinhKhanhGuide.Domain.Entities;
using VinhKhanhGuide.Infrastructure.Persistence.Configurations;

namespace VinhKhanhGuide.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Stall> Stalls => Set<Stall>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StallConfiguration());
    }
}
