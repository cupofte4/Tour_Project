namespace VinhKhanhGuide.Mobile.Models;

public sealed class AppUsageAnalyticsOptions
{
    public const string SectionName = "Analytics";

    public bool Enabled { get; set; } = true;

    public int HeartbeatIntervalSeconds { get; set; } = 60;
}
