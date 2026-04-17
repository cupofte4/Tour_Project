using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Infrastructure.Persistence;

namespace VinhKhanhGuide.Api.Tests;

public sealed class StartupSyncBehaviorTests
{
    [Fact]
    public void CreateClient_DoesNotRunStartupSync_WhenImportIsDisabled()
    {
        using var factory = new StartupSyncWebApplicationFactory(enabled: false, runOnStartup: true);

        using var client = factory.CreateClient();

        Assert.Equal(0, factory.SyncService.CallCount);
    }

    [Fact]
    public void CreateClient_DoesNotRunStartupSync_WhenRunOnStartupIsDisabled()
    {
        using var factory = new StartupSyncWebApplicationFactory(enabled: true, runOnStartup: false);

        using var client = factory.CreateClient();

        Assert.Equal(0, factory.SyncService.CallCount);
    }

    [Fact]
    public void CreateClient_RunsStartupSync_WhenImportIsEnabledAndRunOnStartupIsEnabled()
    {
        using var factory = new StartupSyncWebApplicationFactory(enabled: true, runOnStartup: true);

        using var client = factory.CreateClient();

        Assert.Equal(1, factory.SyncService.CallCount);
    }

    private sealed class StartupSyncWebApplicationFactory(bool enabled, bool runOnStartup) : WebApplicationFactory<Program>
    {
        public CountingRemoteLocationSyncService SyncService { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RemoteLocationImport:Enabled"] = enabled.ToString(),
                    ["RemoteLocationImport:RunOnStartup"] = runOnStartup.ToString(),
                    ["RemoteLocationImport:SqlSnapshotPath"] = "foodguide.sql"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));

                services.RemoveAll<IRemoteLocationSyncService>();
                services.AddSingleton<IRemoteLocationSyncService>(SyncService);
            });
        }
    }

    public sealed class CountingRemoteLocationSyncService : IRemoteLocationSyncService
    {
        public int CallCount { get; private set; }

        public Task<RemoteLocationSyncResult> SyncAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;

            return Task.FromResult(new RemoteLocationSyncResult());
        }
    }
}
