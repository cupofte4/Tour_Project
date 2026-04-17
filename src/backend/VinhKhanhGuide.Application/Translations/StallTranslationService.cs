using VinhKhanhGuide.Application.Stalls;

namespace VinhKhanhGuide.Application.Translations;

public class StallTranslationService(
    IStallReadRepository stallReadRepository,
    IStallTranslationRepository stallTranslationRepository,
    ITranslationService translationService) : IStallTranslationService
{
    public async Task<StallTranslationDto?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        // Live generation is intentionally disabled in the request path. We only
        // serve persisted translations or an explicit miss that the client can handle.
        _ = translationService;
        var normalizedLanguageCode = languageCode.Trim().ToLowerInvariant();

        if (!IsSupportedLanguage(normalizedLanguageCode))
        {
            return null;
        }

        var stall = await stallReadRepository.GetByIdAsync(stallId, cancellationToken);

        if (stall is null)
        {
            return null;
        }

        if (normalizedLanguageCode == "vi")
        {
            return CreateVietnameseSourceTranslation(stall, normalizedLanguageCode, usedFallback: false);
        }

        var storedTranslation = await stallTranslationRepository.GetByStallIdAndLanguageAsync(
            stallId,
            normalizedLanguageCode,
            cancellationToken);

        if (storedTranslation is not null)
        {
            return new StallTranslationDto
            {
                StallId = storedTranslation.StallId,
                RequestedLanguageCode = normalizedLanguageCode,
                LanguageCode = storedTranslation.LanguageCode,
                Name = storedTranslation.Name,
                Description = storedTranslation.Description,
                UsedFallback = false,
                Source = "stored"
            };
        }

        return null;
    }

    private static bool IsSupportedLanguage(string languageCode)
    {
        var normalized = languageCode.Contains('-')
            ? languageCode.Split('-', 2)[0]
            : languageCode;

        return normalized is "vi" or "en" or "zh" or "de";
    }

    private static StallTranslationDto CreateVietnameseSourceTranslation(
        StallDto stall,
        string requestedLanguageCode,
        bool usedFallback)
    {
        return new StallTranslationDto
        {
            StallId = stall.Id,
            RequestedLanguageCode = requestedLanguageCode,
            LanguageCode = "vi",
            Name = stall.Name,
            Description = stall.NarrationScriptVi,
            UsedFallback = usedFallback,
            Source = usedFallback
                ? "vietnamese-source-fallback"
                : "vietnamese-source"
        };
    }
}
