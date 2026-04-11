using Microsoft.Extensions.DependencyInjection;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IStallDistanceCalculator, StallDistanceCalculator>();
        services.AddScoped<IStallReadService, StallReadService>();
        services.AddScoped<IStallTranslationService, StallTranslationService>();
        services.AddScoped<IRemoteLocationSyncService, RemoteLocationSyncService>();

        return services;
    }
}
