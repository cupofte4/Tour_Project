namespace VinhKhanhGuide.Application.Analytics;

public sealed class AppUsageHeartbeatRequest
{
    public string SessionId { get; init; } = string.Empty;

    public string DeviceId { get; init; } = string.Empty;

    public DateTimeOffset? OccurredAtUtc { get; init; }

    public string Platform { get; init; } = string.Empty;

    public string AppVersion { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public string EventType { get; init; } = "heartbeat";

    public string UserAgent { get; init; } = string.Empty;
}
