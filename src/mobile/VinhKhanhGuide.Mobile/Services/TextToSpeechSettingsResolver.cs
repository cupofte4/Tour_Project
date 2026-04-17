using Microsoft.Maui.Media;
using System.Globalization;
using System.Reflection;

namespace VinhKhanhGuide.Mobile.Services;

public static class TextToSpeechSettingsResolver
{
    private static readonly string[] SupportedNarrationLanguages = ["vi", "en", "zh", "de"];
    private static readonly Dictionary<string, string[]> PreferredLocaleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vi"] = ["vi-VN", "vi"],
        ["en"] = ["en-US", "en-GB", "en"],
        ["zh"] = ["zh-CN", "zh-Hans", "zh-TW", "zh-Hant", "zh-HK", "zh"],
        ["de"] = ["de-DE", "de-AT", "de-CH", "de"]
    };

    public static IReadOnlyList<string> SupportedLanguageCodes => SupportedNarrationLanguages;

    public static Locale? ResolvePreferredLocale(
        string preferredLanguage,
        string? preferredLocaleCode,
        IEnumerable<Locale> locales)
    {
        var availableLocales = locales.ToList();
        var selectedCode = ResolvePreferredLocaleCode(
            preferredLanguage,
            preferredLocaleCode,
            availableLocales.Select(locale => locale.Name));

        if (string.IsNullOrWhiteSpace(selectedCode))
        {
            return null;
        }

        return availableLocales.FirstOrDefault(locale =>
            string.Equals(locale.Name, selectedCode, StringComparison.OrdinalIgnoreCase));
    }

    public static Locale CreateLocale(string localeCode)
    {
        var normalizedLocaleCode = NormalizeLocaleCode(localeCode);
        var languageCode = normalizedLocaleCode.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
        var countryCode = string.Empty;

        try
        {
            var culture = CultureInfo.GetCultureInfo(normalizedLocaleCode);
            languageCode = culture.TwoLetterISOLanguageName;

            if (culture.Name.Contains('-', StringComparison.Ordinal))
            {
                var suffix = culture.Name.Split('-', 2)[1];
                if (suffix.Length == 2)
                {
                    countryCode = suffix;
                }
            }
        }
        catch (CultureNotFoundException)
        {
            if (normalizedLocaleCode.Contains('-', StringComparison.Ordinal))
            {
                var suffix = normalizedLocaleCode.Split('-', 2)[1];
                if (suffix.Length == 2)
                {
                    countryCode = suffix;
                }
            }
        }

        var localeConstructor = typeof(Locale).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            [typeof(string), typeof(string), typeof(string), typeof(string)],
            modifiers: null);

        if (localeConstructor is null)
        {
            throw new InvalidOperationException("The platform Locale constructor could not be resolved.");
        }

        return (Locale)localeConstructor.Invoke([languageCode, countryCode, normalizedLocaleCode, normalizedLocaleCode]);
    }

    public static string? ResolvePreferredLocaleCode(
        string preferredLanguage,
        string? preferredLocaleCode,
        IEnumerable<string> localeCodes)
    {
        if (string.IsNullOrWhiteSpace(preferredLanguage))
        {
            return null;
        }

        var resolvedLanguage = ResolveSupportedLanguageCode(preferredLanguage);
        var availableCodes = localeCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(NormalizeLocaleCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!string.IsNullOrWhiteSpace(preferredLocaleCode))
        {
            var exactPreferredLocale = availableCodes.FirstOrDefault(code =>
                string.Equals(code, preferredLocaleCode, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(exactPreferredLocale))
            {
                return exactPreferredLocale;
            }
        }

        foreach (var candidate in GetPreferredLocaleCodes(resolvedLanguage))
        {
            var exactMatch = availableCodes.FirstOrDefault(code =>
                string.Equals(code, candidate, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(exactMatch))
            {
                return exactMatch;
            }
        }

        return availableCodes.FirstOrDefault(code =>
        {
            var localePrefix = code.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
            return string.Equals(localePrefix, resolvedLanguage, StringComparison.OrdinalIgnoreCase);
        });
    }

    public static bool ShouldPreferTextToSpeech(string? resolvedLanguageCode)
    {
        var normalizedLanguage = ResolveSupportedLanguageCode(resolvedLanguageCode);
        return !string.Equals(normalizedLanguage, "vi", StringComparison.OrdinalIgnoreCase);
    }

    public static string GetLanguageDisplayName(string? languageCode)
    {
        return ResolveSupportedLanguageCode(languageCode) switch
        {
            "vi" => "Vietnamese",
            "en" => "English",
            "zh" => "Chinese",
            "de" => "German",
            var code => code
        };
    }

    public static string ResolvePreferredLocaleHint(string? requestedLanguage)
    {
        var resolvedLanguage = ResolveSupportedLanguageCode(requestedLanguage);
        return GetPreferredLocaleCodes(resolvedLanguage).First();
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

        if (!string.Equals(resolvedLanguage, "vi", StringComparison.OrdinalIgnoreCase) &&
            !chain.Contains("vi", StringComparer.OrdinalIgnoreCase))
        {
            chain.Add("vi");
        }

        return chain;
    }

    private static IReadOnlyList<string> GetPreferredLocaleCodes(string resolvedLanguage)
    {
        if (PreferredLocaleMap.TryGetValue(resolvedLanguage, out var localeCodes) &&
            localeCodes.Length > 0)
        {
            return localeCodes;
        }

        return [resolvedLanguage];
    }

    public static string NormalizeLocaleCode(string localeCode)
    {
        return localeCode.Trim().Replace('_', '-');
    }
}
