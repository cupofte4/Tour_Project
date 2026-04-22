namespace VinhKhanhGuide.Application.Analytics;

public sealed class FavoriteClickRequest
{
    public string DeviceId { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public bool IsFavorite { get; init; } = true;
    public DateTimeOffset? OccurredAtUtc { get; init; }
}