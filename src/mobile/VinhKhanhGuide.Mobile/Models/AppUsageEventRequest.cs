namespace VinhKhanhGuide.Mobile.Models;

public sealed class AppUsageEventRequest
{
    public string SessionId { get; init; } = string.Empty;

    public string AnonymousClientId { get; init; } = string.Empty;

    public string EventType { get; init; } = string.Empty;

    public DateTimeOffset? OccurredAtUtc { get; init; }

    public string Platform { get; init; } = string.Empty;

    public string AppVersion { get; init; } = string.Empty;
}
