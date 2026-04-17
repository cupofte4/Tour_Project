namespace VinhKhanhGuide.Application.Analytics;

public interface IAppUsageAnalyticsService
{
    Task RecordEventAsync(AppUsageEventIngestRequest request, CancellationToken cancellationToken = default);

    Task RecordHeartbeatAsync(AppUsageHeartbeatRequest request, CancellationToken cancellationToken = default);

    Task<ActiveUsersMetricDto> GetActiveUsersAsync(CancellationToken cancellationToken = default);
}
