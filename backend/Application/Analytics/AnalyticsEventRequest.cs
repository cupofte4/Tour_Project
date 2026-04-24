namespace VinhKhanhGuide.Application.Analytics;

public sealed class AnalyticsEventRequest
{
    public string DeviceId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public int? LocationId { get; init; }
    public string Path { get; init; } = string.Empty;
    public DateTimeOffset? CreatedAtUtc { get; init; }
}
