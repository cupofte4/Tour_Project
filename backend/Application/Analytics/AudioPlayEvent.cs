namespace VinhKhanhGuide.Application.Analytics;

public sealed class AudioPlayEvent
{
    public string DeviceId { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public int? AudioId { get; init; }
    public DateTimeOffset? OccurredAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
