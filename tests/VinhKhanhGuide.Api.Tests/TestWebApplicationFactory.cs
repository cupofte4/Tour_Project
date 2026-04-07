using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VinhKhanhGuide.Domain.Entities;
using VinhKhanhGuide.Infrastructure.Persistence;
using VinhKhanhGuide.Infrastructure.Persistence.Seeding;

namespace VinhKhanhGuide.Api.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"VinhKhanhGuideApiTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            if (!dbContext.Stalls.Any())
            {
                dbContext.Stalls.AddRange(StallSeedData.Stalls.Select(CreateSeedCopy));
                dbContext.SaveChanges();
            }
        });
    }

    private static Stall CreateSeedCopy(Stall stall)
    {
        return new Stall
        {
            Id = stall.Id,
            Name = stall.Name,
            DescriptionVi = stall.DescriptionVi,
            Latitude = stall.Latitude,
            Longitude = stall.Longitude,
            TriggerRadiusMeters = stall.TriggerRadiusMeters,
            Category = stall.Category,
            OpenHours = stall.OpenHours,
            ImageUrl = stall.ImageUrl,
            AverageRating = stall.AverageRating
        };
    }
}
