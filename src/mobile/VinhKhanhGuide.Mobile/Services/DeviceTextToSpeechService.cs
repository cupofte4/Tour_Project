using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using VinhKhanhGuide.Mobile.Models;
#if IOS
using AVFoundation;
#endif

namespace VinhKhanhGuide.Mobile.Services;

public sealed class DeviceTextToSpeechService : IDeviceTextToSpeech
{
    private readonly ISettingsService _settingsService;

    public DeviceTextToSpeechService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task SpeakAsync(string text, string? languageCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var settings = _settingsService.GetSettings();
        var speechOptions = await CreateSpeechOptionsAsync(settings, languageCode, cancellationToken);

#if IOS
        ConfigureIosAudioSession();
#endif

        await MainThread.InvokeOnMainThreadAsync(() =>
            TextToSpeech.Default.SpeakAsync(text, speechOptions, cancellationToken));
    }

    private static async Task<SpeechOptions?> CreateSpeechOptionsAsync(
        AppSettings settings,
        string? requestedLanguageCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            cancellationToken.ThrowIfCancellationRequested();
            var preferredLanguage = !string.IsNullOrWhiteSpace(requestedLanguageCode)
                ? requestedLanguageCode
                : settings.PreferredTtsLanguage;
            var resolvedLanguage = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(preferredLanguage);
            var locale = TextToSpeechSettingsResolver.ResolvePreferredLocale(resolvedLanguage, locales);

            if (locale is null)
            {
                return null;
            }

            return new SpeechOptions
            {
                Locale = locale
            };
        }
        catch
        {
            return null;
        }
    }

#if IOS
    private static void ConfigureIosAudioSession()
    {
        try
        {
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Playback);
            audioSession.SetActive(true);
        }
        catch
        {
        }
    }
#endif
}
