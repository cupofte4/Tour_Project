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
            options.TriggerDebounce = TimeSpan.FromSeconds(5);
            options.TriggerCooldown = TimeSpan.FromMinutes(3);
        });
        builder.Services.Configure<AutoNarrationOptions>(options =>
        {
            options.Enabled = true;
            options.ReplayCooldown = TimeSpan.FromMinutes(3);
        });
        builder.Services.Configure<LocationTrackingOptions>(options =>
        {
            options.Accuracy = Microsoft.Maui.Devices.Sensors.GeolocationAccuracy.Medium;
            options.UpdateInterval = TimeSpan.FromSeconds(10);
            options.RequestTimeout = TimeSpan.FromSeconds(10);
            options.UseLastKnownLocationFirst = true;
            options.EnableBackgroundUpdates = false;
        });

        builder.Services.AddSingleton<IStallApiClient, StallApiClient>();
        builder.Services.AddSingleton<ISettingsStorage, PreferencesSettingsStorage>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ITtsSettingsSupportService, TtsSettingsSupportService>();
        builder.Services.AddSingleton<ILocationService, DeviceLocationService>();
        builder.Services.AddSingleton<ILocationTrackingService>(sp =>
            (DeviceLocationService)sp.GetRequiredService<ILocationService>());
        builder.Services.AddSingleton<IDeviceTextToSpeech, DeviceTextToSpeechService>();
        builder.Services.AddSingleton<IAudioNarrationPlayer, NullAudioNarrationPlayer>();
        builder.Services.AddSingleton<NarrationSessionManager>();
        builder.Services.AddSingleton<INarrationService, NarrationService>();
        builder.Services.AddSingleton<IProximityDistanceCalculator, ProximityDistanceCalculator>();
        builder.Services.AddSingleton<IProximityService, ProximityService>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IProximityNarrationCoordinator, ProximityNarrationCoordinator>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<StallListViewModel>();
        builder.Services.AddTransient<StallDetailViewModel>();
        builder.Services.AddTransient<StallMapViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<StallDetailPage>();
        builder.Services.AddTransient<StallMapPage>();
        builder.Services.AddTransient<SettingsPage>();

        var app = builder.Build();
        ServiceHelper.Services = app.Services;

        return app;
    }
}
