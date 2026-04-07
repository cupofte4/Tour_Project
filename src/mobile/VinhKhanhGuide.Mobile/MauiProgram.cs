using Microsoft.Extensions.DependencyInjection;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;
using VinhKhanhGuide.Mobile.Views;

namespace VinhKhanhGuide.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
            });

        builder.Services.Configure<StallApiOptions>(options =>
        {
            options.BaseUrl = "http://localhost:5113/";
        });
        builder.Services.Configure<ProximityOptions>(options =>
        {
            options.TriggerCooldown = TimeSpan.FromMinutes(3);
        });

        builder.Services.AddSingleton<IStallApiClient, StallApiClient>();
        builder.Services.AddSingleton<ILocationService, DeviceLocationService>();
        builder.Services.AddSingleton<IProximityDistanceCalculator, ProximityDistanceCalculator>();
        builder.Services.AddSingleton<IProximityService, ProximityService>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<StallListViewModel>();
        builder.Services.AddTransient<StallDetailViewModel>();
        builder.Services.AddTransient<StallMapViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<StallDetailPage>();
        builder.Services.AddTransient<StallMapPage>();

        var app = builder.Build();
        ServiceHelper.Services = app.Services;

        return app;
    }
}
