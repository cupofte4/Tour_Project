using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface ITtsSettingsSupportService
{
    Task<IReadOnlyList<TtsLanguageOption>> GetSupportedLanguagesAsync(CancellationToken cancellationToken = default);
}
