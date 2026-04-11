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
        foreach (var candidateLanguage in TextToSpeechSettingsResolver.BuildLanguageFallbackChain(
                     requestedLanguageCode,
                     SupportedLanguageCodes))
        {
            if (string.Equals(candidateLanguage, "vi", StringComparison.OrdinalIgnoreCase))
            {
                return CreateResolvedNarration(defaultName, fallbackVietnameseText, audioUrl, "vi");
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
                return CreateResolvedNarration(name, narrationText, audioUrl, candidateLanguage);
            }
        }

        return CreateResolvedNarration(defaultName, fallbackVietnameseText, audioUrl, "vi");
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
        string languageCode)
    {
        var normalizedText = text?.Trim() ?? string.Empty;
        var normalizedAudioUrl = audioUrl?.Trim() ?? string.Empty;
        var hasText = !string.IsNullOrWhiteSpace(normalizedText);

        return new ResolvedStallNarration(
            string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim(),
            normalizedText,
            TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(languageCode, SupportedLanguageCodes),
            hasText ? string.Empty : normalizedAudioUrl,
            hasText,
            hasText || !string.IsNullOrWhiteSpace(normalizedAudioUrl));
    }
}

public sealed record ResolvedStallNarration(
    string Name,
    string Text,
    string LanguageCode,
    string AudioUrl,
    bool HasText,
    bool CanNarrate);
