namespace VinhKhanhGuide.Mobile.Models;

public sealed class NarrationPlaybackState
{
    public NarrationPlaybackStatus Status { get; init; } = NarrationPlaybackStatus.Idle;

    public int? ActivePoiId { get; init; }

    public NarrationContentKind? ContentKind { get; init; }

    public string RequestedLanguageCode { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = string.Empty;

    public string LocaleCode { get; init; } = string.Empty;

    public bool UsedFallback { get; init; }

    public string Message { get; init; } = string.Empty;

    public static NarrationPlaybackState Idle { get; } = new();
}
