namespace VinhKhanhGuide.Infrastructure.Analytics;

public sealed class AppUsageAnalyticsOptions
{
    public const string SectionName = "Analytics";

    public bool Enabled { get; set; } = true;

    public int ActiveUserWindowMinutes { get; set; } = 3;
}
