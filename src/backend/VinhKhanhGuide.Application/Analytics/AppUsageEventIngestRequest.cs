namespace VinhKhanhGuide.Application.Analytics;

public sealed class AppUsageEventIngestRequest
{
    public string SessionId { get; init; } = string.Empty;

    public string AnonymousClientId { get; init; } = string.Empty;

    public string EventType { get; init; } = string.Empty;

    public DateTimeOffset? OccurredAtUtc { get; init; }

    public string Platform { get; init; } = string.Empty;

    public string AppVersion { get; init; } = string.Empty;
}
