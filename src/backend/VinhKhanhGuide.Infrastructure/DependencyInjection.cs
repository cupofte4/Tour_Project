using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;
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
            options.SqlSnapshotPath = importSection["SqlSnapshotPath"] ?? options.SqlSnapshotPath;
        });
        services.AddScoped<IStallReadRepository, StallReadRepository>();
        services.AddScoped<IStallContentSyncRepository>(serviceProvider =>
            (StallReadRepository)serviceProvider.GetRequiredService<IStallReadRepository>());
        services.AddScoped<IStallTranslationRepository, StallTranslationRepository>();
        services.AddSingleton<IRemoteLocationContentSource, SqlScriptRemoteLocationContentSource>();
        services.AddSingleton<FakeTranslationService>();
        services.AddSingleton<ITranslationService>(serviceProvider =>
            serviceProvider.GetRequiredService<FakeTranslationService>());

        return services;
    }
}
