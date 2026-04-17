using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public delegate void AppSettingsChangedEventHandler(AppSettings settings);

public interface ISettingsService
{
    event AppSettingsChangedEventHandler? SettingsChanged;

    AppSettings GetSettings();

    void SaveSettings(AppSettings settings);

    string GetResolvedApiBaseUrl();

    string GetConfiguredApiBaseUrl();
}
