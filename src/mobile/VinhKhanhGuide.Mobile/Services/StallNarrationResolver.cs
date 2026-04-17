using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public static class StallNarrationResolver
{
    public static IReadOnlyList<string> SupportedLanguageCodes => TextToSpeechSettingsResolver.SupportedLanguageCodes;

    public static async Task<ResolvedStallNarration> ResolveAsync(
        string? requestedLanguageCode,
        string defaultName,
        string fallbackVietnameseText,
        string audioUrl,
        Func<string, Task<StallTranslation?>> translationLoaderAsync)
    {
        var normalizedRequestedLanguage = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(
            requestedLanguageCode,
            SupportedLanguageCodes);

        foreach (var candidateLanguage in TextToSpeechSettingsResolver.BuildLanguageFallbackChain(
                     normalizedRequestedLanguage,
                     SupportedLanguageCodes))
        {
            if (string.Equals(candidateLanguage, "vi", StringComparison.OrdinalIgnoreCase))
            {
                return CreateResolvedNarration(
                    defaultName,
                    fallbackVietnameseText,
                    audioUrl,
                    normalizedRequestedLanguage,
                    "vi");
            }

            var translation = await translationLoaderAsync(candidateLanguage);

            if (translation is null)
            {
                continue;
            }

            var narrationText = string.IsNullOrWhiteSpace(translation.Description)
                ? string.Empty
                : translation.Description.Trim();

            var name = string.IsNullOrWhiteSpace(translation.Name)
                ? defaultName
                : translation.Name.Trim();

            if (!string.IsNullOrWhiteSpace(narrationText))
            {
                return CreateResolvedNarration(
                    name,
                    narrationText,
                    audioUrl,
                    normalizedRequestedLanguage,
                    candidateLanguage);
            }
        }

        return CreateResolvedNarration(
            defaultName,
            fallbackVietnameseText,
            audioUrl,
            normalizedRequestedLanguage,
            "vi");
    }

    public static string ResolveVietnameseNarrationText(string narrationScriptVi, string descriptionVi)
    {
        return !string.IsNullOrWhiteSpace(narrationScriptVi)
            ? narrationScriptVi.Trim()
            : descriptionVi.Trim();
    }

    private static ResolvedStallNarration CreateResolvedNarration(
        string name,
        string text,
        string audioUrl,
        string requestedLanguageCode,
        string resolvedLanguageCode)
    {
        var normalizedText = text?.Trim() ?? string.Empty;
        var normalizedAudioUrl = audioUrl?.Trim() ?? string.Empty;
        var hasText = !string.IsNullOrWhiteSpace(normalizedText);
        var normalizedRequestedLanguage = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(
            requestedLanguageCode,
            SupportedLanguageCodes);
        var normalizedResolvedLanguage = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(
            resolvedLanguageCode,
            SupportedLanguageCodes);
        var usedFallback = !string.Equals(
            normalizedRequestedLanguage,
            normalizedResolvedLanguage,
            StringComparison.OrdinalIgnoreCase);

        return new ResolvedStallNarration(
            string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim(),
            normalizedText,
            normalizedRequestedLanguage,
            normalizedResolvedLanguage,
            TextToSpeechSettingsResolver.ResolvePreferredLocaleHint(normalizedResolvedLanguage),
            normalizedAudioUrl,
            hasText,
            hasText || !string.IsNullOrWhiteSpace(normalizedAudioUrl),
            usedFallback);
    }
}

public sealed record ResolvedStallNarration(
    string Name,
    string Text,
    string RequestedLanguageCode,
    string LanguageCode,
    string LocaleCode,
    string AudioUrl,
    bool HasText,
    bool CanNarrate,
    bool UsedFallback);
