using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

internal sealed class TestSettingsService : ISettingsService
{
    public event AppSettingsChangedEventHandler? SettingsChanged;

    public AppSettings Settings { get; private set; } = new();

    public string ResolvedApiBaseUrl { get; set; } = string.Empty;

    public AppSettings GetSettings()
    {
        return Settings;
    }

    public void SaveSettings(AppSettings settings)
    {
        Settings = settings;
        SettingsChanged?.Invoke(settings);
    }

    public string GetResolvedApiBaseUrl()
    {
        return ResolvedApiBaseUrl;
    }

    public string ConfiguredApiBaseUrl { get; set; } = string.Empty;

    public string GetConfiguredApiBaseUrl()
    {
        return ConfiguredApiBaseUrl;
    }
}
