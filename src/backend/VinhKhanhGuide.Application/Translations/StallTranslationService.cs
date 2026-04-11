using VinhKhanhGuide.Application.Stalls;

namespace VinhKhanhGuide.Application.Translations;

public class StallTranslationService(
    IStallReadRepository stallReadRepository,
    IStallTranslationRepository stallTranslationRepository,
    ITranslationService translationService) : IStallTranslationService
{
    public async Task<StallTranslationDto?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
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
            return new StallTranslationDto
            {
                StallId = stall.Id,
                LanguageCode = "vi",
                Name = stall.Name,
                Description = stall.NarrationScriptVi
            };
        }

        foreach (var candidateLanguage in BuildFallbackChain(normalizedLanguageCode))
        {
            if (candidateLanguage == "vi")
            {
                return new StallTranslationDto
                {
                    StallId = stall.Id,
                    LanguageCode = "vi",
                    Name = stall.Name,
                    Description = stall.NarrationScriptVi
                };
            }

            var cachedTranslation = await stallTranslationRepository.GetByStallIdAndLanguageAsync(
                stallId,
                candidateLanguage,
                cancellationToken);

            if (cachedTranslation is not null)
            {
                return cachedTranslation;
            }
        }

        return new StallTranslationDto
        {
            StallId = stall.Id,
            LanguageCode = "vi",
            Name = stall.Name,
            Description = stall.NarrationScriptVi
        };
    }

    private static IReadOnlyList<string> BuildFallbackChain(string languageCode)
    {
        var candidates = new List<string>();
        AddCandidate(candidates, languageCode);

        if (!string.Equals(languageCode, "en", StringComparison.OrdinalIgnoreCase))
        {
            AddCandidate(candidates, "en");
        }

        AddCandidate(candidates, "vi");
        return candidates;
    }

    private static void AddCandidate(ICollection<string> candidates, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return;
        }

        var normalized = languageCode.Trim().ToLowerInvariant();

        if (normalized.Contains('-'))
        {
            normalized = normalized.Split('-', 2)[0];
        }

        if (!candidates.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            candidates.Add(normalized);
        }
    }

    private static bool IsSupportedLanguage(string languageCode)
    {
        var normalized = languageCode.Contains('-')
            ? languageCode.Split('-', 2)[0]
            : languageCode;

        return normalized is "vi" or "en" or "zh" or "de";
    }
}
