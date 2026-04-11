namespace VinhKhanhGuide.Mobile.Models;

public sealed class NarrationRequest
{
    public int PoiId { get; init; }

    public int Priority { get; init; }

    public string Title { get; init; } = string.Empty;

    public string AudioUrl { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = string.Empty;
}
