namespace VinhKhanhGuide.Application.Analytics;

public interface IAnalyticsService
{
    Task RecordAudioPlayAsync(AudioPlayRequest request, CancellationToken cancellationToken = default);

    Task RecordFavoriteClickAsync(FavoriteClickRequest request, CancellationToken cancellationToken = default);

    Task RecordHeartbeatAsync(AppUsageHeartbeatRequest request, CancellationToken cancellationToken = default);

    Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<DailyMetricsDto>> GetChartDataAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

public sealed class AnalyticsSummaryDto
{
    public int CurrentActiveDevices { get; init; }
    public long TotalAudioPlays { get; init; }
    public long TotalFavoritesSaved { get; init; }
}

public sealed class DailyMetricsDto
{
    public DateTime Date { get; init; }
    public long AudioPlays { get; init; }
    public long Favorites { get; init; }
}