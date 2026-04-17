using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VinhKhanhGuide.Application.Analytics;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;
using VinhKhanhGuide.Infrastructure.Analytics;
using VinhKhanhGuide.Infrastructure.Persistence;
using VinhKhanhGuide.Infrastructure.RemoteLocations;
using VinhKhanhGuide.Infrastructure.Stalls;
using VinhKhanhGuide.Infrastructure.Translations;

namespace VinhKhanhGuide.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<RemoteLocationImportOptions>(options =>
        {
            var importSection = configuration.GetSection("RemoteLocationImport");

            if (bool.TryParse(importSection["Enabled"], out var enabled))
            {
                options.Enabled = enabled;
            }

            if (bool.TryParse(importSection["RunOnStartup"], out var runOnStartup))
            {
                options.RunOnStartup = runOnStartup;
            }

            options.SqlSnapshotPath = importSection["SqlSnapshotPath"] ?? options.SqlSnapshotPath;
        });
        services.Configure<AppUsageAnalyticsOptions>(options =>
        {
            var analyticsSection = configuration.GetSection(AppUsageAnalyticsOptions.SectionName);

            if (bool.TryParse(analyticsSection["Enabled"], out var enabled))
            {
                options.Enabled = enabled;
            }

            if (int.TryParse(analyticsSection["ActiveUserWindowMinutes"], out var activeUserWindowMinutes))
            {
                options.ActiveUserWindowMinutes = activeUserWindowMinutes;
            }
        });
        services.AddScoped<IStallReadRepository, StallReadRepository>();
        services.AddScoped<IStallContentSyncRepository>(serviceProvider =>
            (StallReadRepository)serviceProvider.GetRequiredService<IStallReadRepository>());
        services.AddScoped<IStallTranslationRepository, StallTranslationRepository>();
        services.AddSingleton<IRemoteLocationContentSource, SqlScriptRemoteLocationContentSource>();
        services.AddSingleton<FakeTranslationService>();
        services.AddSingleton<ITranslationService>(serviceProvider =>
            serviceProvider.GetRequiredService<FakeTranslationService>());
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAppUsageAnalyticsService, AppUsageAnalyticsService>();

        return services;
    }
}
