using System.Globalization;
using Microsoft.Maui.Media;
#if IOS
using AVFoundation;
#endif

namespace VinhKhanhGuide.Mobile.Services;

public interface ITtsLocaleDiscoveryService
{
    Task<IReadOnlyList<Locale>> GetAvailableLocalesAsync(CancellationToken cancellationToken = default);
}

public sealed class TtsLocaleDiscoveryService : ITtsLocaleDiscoveryService
{
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private readonly Func<CancellationToken, Task<IReadOnlyList<Locale>>> _mauiLocalesProvider;
    private readonly Func<IReadOnlyList<Locale>> _iosLocalesProvider;
    private IReadOnlyList<Locale>? _cachedLocales;

    public TtsLocaleDiscoveryService()
        : this(
            async cancellationToken => (await TextToSpeech.Default.GetLocalesAsync().WaitAsync(cancellationToken)).ToArray(),
            GetIosLocales)
    {
    }

    internal TtsLocaleDiscoveryService(
        Func<CancellationToken, Task<IReadOnlyList<Locale>>> mauiLocalesProvider,
        Func<IReadOnlyList<Locale>> iosLocalesProvider)
    {
        _mauiLocalesProvider = mauiLocalesProvider;
        _iosLocalesProvider = iosLocalesProvider;
    }

    public async Task<IReadOnlyList<Locale>> GetAvailableLocalesAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedLocales is not null)
        {
            return _cachedLocales;
        }

        await _cacheLock.WaitAsync(cancellationToken);

        try
        {
            if (_cachedLocales is not null)
            {
                return _cachedLocales;
            }

            Exception? iosException = null;
            Exception? mauiException = null;

            try
            {
                var iosLocales = NormalizeLocales(_iosLocalesProvider());
                if (iosLocales.Count > 0)
                {
                    _cachedLocales = iosLocales;
                    return iosLocales;
                }
            }
            catch (Exception exception)
            {
                iosException = exception;
            }

            try
            {
                var mauiLocales = NormalizeLocales(await _mauiLocalesProvider(cancellationToken));
                if (mauiLocales.Count > 0)
                {
                    _cachedLocales = mauiLocales;
                    return mauiLocales;
                }
            }
            catch (Exception exception)
            {
                mauiException = exception;
            }

            if (iosException is not null || mauiException is not null)
            {
                throw new TextToSpeechLocaleDiscoveryException(
                    "Text-to-speech voices couldn't be discovered on this device.",
                    iosException ?? mauiException);
            }

            return [];
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private static IReadOnlyList<Locale> NormalizeLocales(IEnumerable<Locale> locales)
    {
        return locales
            .Where(locale => locale is not null && !string.IsNullOrWhiteSpace(locale.Name))
            .Select(locale => TextToSpeechSettingsResolver.CreateLocale(locale.Name))
            .GroupBy(locale => locale.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }

    private static IReadOnlyList<Locale> GetIosLocales()
    {
#if IOS
        return AVSpeechSynthesisVoice.GetSpeechVoices()
            .Where(voice => !string.IsNullOrWhiteSpace(voice.Language))
            .Select(voice => TextToSpeechSettingsResolver.CreateLocale(voice.Language))
            .GroupBy(locale => locale.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
#else
        return [];
#endif
    }
}
