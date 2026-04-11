namespace VinhKhanhGuide.Mobile.Models;

public class StallTranslation
{
    public int StallId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}
