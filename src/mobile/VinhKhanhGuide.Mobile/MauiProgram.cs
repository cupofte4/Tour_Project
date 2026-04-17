using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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

        AddAppConfiguration(builder.Configuration);

        builder.Services.Configure<StallApiOptions>(options =>
        {
            options.BaseUrl = builder.Configuration[$"{StallApiOptions.SectionName}:BaseUrl"] ?? options.BaseUrl;
        });
        builder.Services.Configure<AppUsageAnalyticsOptions>(options =>
        {
            if (bool.TryParse(builder.Configuration[$"{AppUsageAnalyticsOptions.SectionName}:Enabled"], out var enabled))
            {
                options.Enabled = enabled;
            }

            if (int.TryParse(builder.Configuration[$"{AppUsageAnalyticsOptions.SectionName}:HeartbeatIntervalSeconds"], out var heartbeatIntervalSeconds))
            {
                options.HeartbeatIntervalSeconds = heartbeatIntervalSeconds;
            }
        });
        builder.Services.PostConfigure<StallApiOptions>(options =>
        {
            // The bundled config is intentionally allowed to stay blank so device/demo builds do not
            // silently point at localhost. A persisted override or environment-specific config can supply it.
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                options.BaseUrl = string.Empty;
                return;
            }

            if (!StallApiOptions.TryNormalizeBaseUrl(options.BaseUrl, out var normalizedBaseUrl))
            {
                throw new InvalidOperationException(
                    $"Configuration section '{StallApiOptions.SectionName}' must contain a valid absolute BaseUrl.");
            }

            options.BaseUrl = normalizedBaseUrl;
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
        builder.Services.AddSingleton<IStallDataCache, SqliteStallDataCache>();
        builder.Services.AddSingleton<IStallDataService, CachedStallDataService>();
        builder.Services.AddSingleton<IExternalStallRouteParser, ExternalStallRouteParser>();
        builder.Services.AddSingleton<IAppNavigator, ShellAppNavigator>();
        builder.Services.AddSingleton<ExternalEntryStatusService>();
        builder.Services.AddSingleton<IExternalEntryService, ExternalEntryService>();
        builder.Services.AddSingleton<ISettingsStorage, PreferencesSettingsStorage>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IAppUsageEnvironmentInfo, DeviceAppUsageEnvironmentInfo>();
        builder.Services.AddSingleton<IAnalyticsApiClient, AnalyticsApiClient>();
        builder.Services.AddSingleton<IUsageAnalyticsService, UsageAnalyticsService>();
        builder.Services.AddSingleton<ITtsLocaleDiscoveryService, TtsLocaleDiscoveryService>();
        builder.Services.AddSingleton<ITtsSettingsSupportService, TtsSettingsSupportService>();
        builder.Services.AddSingleton<ILocationService, DeviceLocationService>();
        builder.Services.AddSingleton<ILocationTrackingService>(sp =>
            (DeviceLocationService)sp.GetRequiredService<ILocationService>());
        builder.Services.AddSingleton<IDeviceTextToSpeech, DeviceTextToSpeechService>();
        builder.Services.AddSingleton<IAudioNarrationPlayer, DeviceAudioNarrationPlayer>();
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<IOfflineAudioDownloadService, OfflineAudioDownloadService>();
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

    private static void AddAppConfiguration(ConfigurationManager configuration)
    {
        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(appSettingsPath))
        {
            return;
        }

        using var stream = File.OpenRead(appSettingsPath);
        configuration.AddJsonStream(stream);
    }
}
