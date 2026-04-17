namespace VinhKhanhGuide.Mobile.Models;

public sealed class StallDetailOpenRequest
{
    public int StallId { get; init; }

    public bool AutoPlay { get; init; }

    public string? RequestedLanguageCode { get; init; }
}
