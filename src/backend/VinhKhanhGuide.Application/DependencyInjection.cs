using Microsoft.Extensions.DependencyInjection;
using VinhKhanhGuide.Application.Stalls;

namespace VinhKhanhGuide.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IStallDistanceCalculator, StallDistanceCalculator>();
        services.AddScoped<IStallReadService, StallReadService>();

        return services;
    }
}
