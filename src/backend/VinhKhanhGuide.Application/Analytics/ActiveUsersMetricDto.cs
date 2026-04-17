namespace VinhKhanhGuide.Application.Analytics;

public sealed class ActiveUsersMetricDto
{
    public int ActiveUsers { get; init; }

    public int WindowMinutes { get; init; }

    public DateTimeOffset LastUpdatedUtc { get; init; }
}
