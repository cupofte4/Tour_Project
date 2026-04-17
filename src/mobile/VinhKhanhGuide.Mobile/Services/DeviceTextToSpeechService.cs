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
    private readonly ITtsLocaleDiscoveryService _localeDiscoveryService;

    public DeviceTextToSpeechService(
        ISettingsService settingsService,
        ITtsLocaleDiscoveryService localeDiscoveryService)
    {
        _settingsService = settingsService;
        _localeDiscoveryService = localeDiscoveryService;
    }

    public async Task SpeakAsync(string text, string? languageCode, string? localeCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var settings = _settingsService.GetSettings();
        var speechOptions = await CreateSpeechOptionsAsync(
            settings,
            languageCode,
            localeCode,
            _localeDiscoveryService,
            cancellationToken);

#if IOS
        ConfigureIosAudioSession();
#endif

        await MainThread.InvokeOnMainThreadAsync(() =>
            TextToSpeech.Default.SpeakAsync(text, speechOptions, cancellationToken));
    }

    private static async Task<SpeechOptions?> CreateSpeechOptionsAsync(
        AppSettings settings,
        string? requestedLanguageCode,
        string? requestedLocaleCode,
        ITtsLocaleDiscoveryService localeDiscoveryService,
        CancellationToken cancellationToken)
    {
        var locales = await localeDiscoveryService.GetAvailableLocalesAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var preferredLanguage = !string.IsNullOrWhiteSpace(requestedLanguageCode)
            ? requestedLanguageCode
            : settings.PreferredTtsLanguage;
        var locale = TextToSpeechSettingsResolver.ResolvePreferredLocale(
            preferredLanguage,
            requestedLocaleCode,
            locales);

        if (locale is null)
        {
            var resolvedLanguage = TextToSpeechSettingsResolver.GetLanguageDisplayName(preferredLanguage);
            throw new TextToSpeechLocaleUnavailableException(
                $"No {resolvedLanguage} text-to-speech voice is available on this device.");
        }

        return new SpeechOptions
        {
            Locale = locale
        };
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
