using Microsoft.EntityFrameworkCore;
using VinhKhanhGuide.Application.Analytics;
using Tour_Project.Data;
using Tour_Project.Models;

namespace VinhKhanhGuide.Infrastructure.Analytics;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _db;

    public AnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task RecordAudioPlayAsync(AudioPlayRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId)) throw new ArgumentException("DeviceId is required.", nameof(request));

        var entity = new AudioPlay
        {
            DeviceId = request.DeviceId.Trim(),
            LocationId = request.LocationId,
            AudioId = request.AudioId,
            OccurredAtUtc = request.OccurredAtUtc ?? DateTimeOffset.UtcNow
        };

        await _db.AudioPlays.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordFavoriteClickAsync(FavoriteClickRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId)) throw new ArgumentException("DeviceId is required.", nameof(request));

        var entity = new FavoriteClick
        {
            DeviceId = request.DeviceId.Trim(),
            LocationId = request.LocationId,
            IsFavorite = request.IsFavorite,
            OccurredAtUtc = request.OccurredAtUtc ?? DateTimeOffset.UtcNow
        };

        await _db.FavoriteClicks.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordHeartbeatAsync(AppUsageHeartbeatRequest request, CancellationToken cancellationToken = default)
    {
        // Heartbeat tracking removed: no-op to satisfy interface while AppUsageSession entity is deleted.
        await Task.CompletedTask;
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var audioPlays = await _db.AudioPlays.AsNoTracking().LongCountAsync(cancellationToken);
        var favorites = await _db.FavoriteClicks.AsNoTracking().LongCountAsync(cancellationToken);

        return new AnalyticsSummaryDto
        {
            CurrentActiveDevices = 0,
            TotalAudioPlays = audioPlays,
            TotalFavoritesSaved = favorites
        };
    }

    public async Task<IEnumerable<DailyMetricsDto>> GetChartDataAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate.Date;
        var end = endDate.Date;

        var audioQuery = _db.AudioPlays.AsNoTracking()
            .Where(x => x.OccurredAtUtc.Date >= start && x.OccurredAtUtc.Date <= end)
            .GroupBy(x => x.OccurredAtUtc.Date)
            .Select(g => new { Date = g.Key, Count = g.LongCount() });

        var favQuery = _db.FavoriteClicks.AsNoTracking()
            .Where(x => x.OccurredAtUtc.Date >= start && x.OccurredAtUtc.Date <= end)
            .GroupBy(x => x.OccurredAtUtc.Date)
            .Select(g => new { Date = g.Key, Count = g.LongCount() });

        var audio = await audioQuery.ToListAsync(cancellationToken);
        var favs = await favQuery.ToListAsync(cancellationToken);

        var result = new List<DailyMetricsDto>();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var a = audio.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
            var f = favs.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
            result.Add(new DailyMetricsDto { Date = date, AudioPlays = a, Favorites = f });
        }

        return result;
    }
}
