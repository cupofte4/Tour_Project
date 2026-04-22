namespace VinhKhanhGuide.Application.Analytics;

public sealed class AudioPlayRequest
{
    public string DeviceId { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public int? AudioId { get; init; }
    public DateTimeOffset? OccurredAtUtc { get; init; }
}