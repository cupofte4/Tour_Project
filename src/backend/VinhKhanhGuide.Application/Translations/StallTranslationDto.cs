namespace VinhKhanhGuide.Application.Translations;

public class StallTranslationDto
{
    public int StallId { get; init; }

    public string RequestedLanguageCode { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public bool UsedFallback { get; init; }

    public string Source { get; init; } = string.Empty;
}
