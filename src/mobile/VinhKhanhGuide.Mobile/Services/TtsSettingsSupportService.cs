using Microsoft.Maui.Media;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class TtsSettingsSupportService : ITtsSettingsSupportService
{
    private static readonly TtsLanguageOption SystemDefaultLanguage = new()
    {
        Code = string.Empty,
        DisplayName = "System Default"
    };

    public async Task<IReadOnlyList<TtsLanguageOption>> GetSupportedLanguagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var languages = locales
                .Select(locale => new TtsLanguageOption
                {
                    Code = locale.Name,
                    DisplayName = string.IsNullOrWhiteSpace(locale.Country)
                        ? locale.Name
                        : $"{locale.Name} ({locale.Country})"
                })
                .GroupBy(option => option.Code, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(option => option.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            languages.Insert(0, SystemDefaultLanguage);
            return languages;
        }
        catch
        {
            return [SystemDefaultLanguage];
        }
    }
}
