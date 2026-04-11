using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Infrastructure.Persistence;
using VinhKhanhGuide.Infrastructure.RemoteLocations;
using VinhKhanhGuide.Infrastructure.Translations;

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
            var options = scope.ServiceProvider.GetRequiredService<IOptions<RemoteLocationImportOptions>>();
            var syncService = scope.ServiceProvider.GetRequiredService<IRemoteLocationSyncService>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            options.Value.SqlSnapshotPath = ResolveFoodGuideSqlPath();
            syncService.SyncAsync().GetAwaiter().GetResult();
        });
    }

    public void ResetTranslationState()
    {
        using var scope = Services.CreateScope();
        var fakeTranslator = scope.ServiceProvider.GetRequiredService<FakeTranslationService>();
        fakeTranslator.Reset();
    }

    private static string ResolveFoodGuideSqlPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "foodguide.sql");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("Could not find foodguide.sql for API tests.");
    }
}
