using Microsoft.Maui.Media;

namespace VinhKhanhGuide.Mobile.Services;

public static class TextToSpeechSettingsResolver
{
    private static readonly string[] SupportedNarrationLanguages = ["vi", "en", "zh", "de"];

    public static IReadOnlyList<string> SupportedLanguageCodes => SupportedNarrationLanguages;

    public static Locale? ResolvePreferredLocale(string preferredLanguage, IEnumerable<Locale> locales)
    {
        var availableLocales = locales.ToList();
        var selectedCode = ResolvePreferredLocaleCode(
            preferredLanguage,
            availableLocales.Select(locale => locale.Name));

        if (string.IsNullOrWhiteSpace(selectedCode))
        {
            return null;
        }

        return availableLocales.FirstOrDefault(locale =>
            string.Equals(locale.Name, selectedCode, StringComparison.OrdinalIgnoreCase));
    }

    public static string? ResolvePreferredLocaleCode(string preferredLanguage, IEnumerable<string> localeCodes)
    {
        if (string.IsNullOrWhiteSpace(preferredLanguage))
        {
            return null;
        }

        var availableCodes = localeCodes.ToList();
        var exactMatch = availableCodes.FirstOrDefault(code =>
            string.Equals(code, preferredLanguage, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(exactMatch))
        {
            return exactMatch;
        }

        var languagePrefix = preferredLanguage.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
        return availableCodes.FirstOrDefault(code =>
        {
            var localePrefix = code.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
            return string.Equals(localePrefix, languagePrefix, StringComparison.OrdinalIgnoreCase);
        });
    }

    public static string ResolveSupportedLanguageCode(string? requestedLanguage)
    {
        return ResolveSupportedLanguageCode(requestedLanguage, SupportedNarrationLanguages);
    }

    public static string ResolveSupportedLanguageCode(string? requestedLanguage, IEnumerable<string> supportedLanguageCodes)
    {
        var supportedCodes = supportedLanguageCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (supportedCodes.Count == 0)
        {
            return "vi";
        }

        if (!string.IsNullOrWhiteSpace(requestedLanguage))
        {
            var exactMatch = supportedCodes.FirstOrDefault(code =>
                string.Equals(code, requestedLanguage, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(exactMatch))
            {
                return exactMatch;
            }

            var requestedPrefix = requestedLanguage.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
            var prefixMatch = supportedCodes.FirstOrDefault(code =>
                string.Equals(code, requestedPrefix, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(prefixMatch))
            {
                return prefixMatch;
            }
        }

        var englishFallback = supportedCodes.FirstOrDefault(code =>
            string.Equals(code, "en", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(englishFallback))
        {
            return englishFallback;
        }

        var vietnameseFallback = supportedCodes.FirstOrDefault(code =>
            string.Equals(code, "vi", StringComparison.OrdinalIgnoreCase));

        return !string.IsNullOrWhiteSpace(vietnameseFallback)
            ? vietnameseFallback
            : supportedCodes[0];
    }

    public static IReadOnlyList<string> BuildLanguageFallbackChain(string? requestedLanguage)
    {
        return BuildLanguageFallbackChain(requestedLanguage, SupportedNarrationLanguages);
    }

    public static IReadOnlyList<string> BuildLanguageFallbackChain(
        string? requestedLanguage,
        IEnumerable<string> supportedLanguageCodes)
    {
        var resolvedLanguage = ResolveSupportedLanguageCode(requestedLanguage, supportedLanguageCodes);
        var chain = new List<string> { resolvedLanguage };

        if (!string.Equals(resolvedLanguage, "en", StringComparison.OrdinalIgnoreCase))
        {
            chain.Add("en");
        }

        if (!string.Equals(resolvedLanguage, "vi", StringComparison.OrdinalIgnoreCase) &&
            !chain.Contains("vi", StringComparer.OrdinalIgnoreCase))
        {
            chain.Add("vi");
        }

        return chain;
    }
}
