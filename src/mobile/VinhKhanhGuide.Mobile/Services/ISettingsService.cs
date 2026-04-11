using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface ISettingsService
{
    AppSettings GetSettings();

    void SaveSettings(AppSettings settings);
}
