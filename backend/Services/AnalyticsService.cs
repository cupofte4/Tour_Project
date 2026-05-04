using Microsoft.EntityFrameworkCore;
using VinhKhanhGuide.Application.Analytics;
using Tour_Project.Data;
using Tour_Project.Models;

namespace VinhKhanhGuide.Infrastructure.Analytics;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _db;
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);
    private static bool _schemaEnsured;
    private static readonly HashSet<string> VisitEventTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "page_view",
        "heartbeat"
    };

    public AnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task RecordAudioPlayAsync(AudioPlayRequest request, CancellationToken cancellationToken = default)
        => await RecordAudioPlayBatchAsync(new[] { request }, cancellationToken);

    public async Task RecordAudioPlayBatchAsync(IEnumerable<AudioPlayRequest> requests, CancellationToken cancellationToken = default)
    {
        await EnsureAnalyticsSchemaAsync(cancellationToken);

        var normalizedRequests = (requests ?? Array.Empty<AudioPlayRequest>())
            .Select(request => new
            {
                Request = request,
                DeviceId = NormalizeDeviceId(request.DeviceId),
                OccurredAtUtc = request.OccurredAtUtc ?? DateTimeOffset.UtcNow
            })
            .ToArray();

        if (normalizedRequests.Length == 0)
        {
            return;
        }

        foreach (var item in normalizedRequests)
        {
            if (string.IsNullOrWhiteSpace(item.DeviceId))
                throw new ArgumentException("DeviceId is required.", nameof(requests));

            if (item.Request.LocationId <= 0)
                throw new ArgumentException("LocationId is required.", nameof(requests));
        }

        var entities = normalizedRequests.Select(item => new AudioPlay
        {
            DeviceId = item.DeviceId,
            LocationId = item.Request.LocationId,
            AudioId = item.Request.AudioId,
            OccurredAtUtc = item.OccurredAtUtc
        }).ToArray();

        await _db.AudioPlays.AddRangeAsync(entities, cancellationToken);

        foreach (var entity in entities)
        {
            await AddEventAsync(entity.DeviceId, "audio_play", entity.LocationId, string.Empty, entity.OccurredAtUtc, cancellationToken);
        }

        await IncrementLocationAudioPlayStatsAsync(entities, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordFavoriteClickAsync(FavoriteClickRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAnalyticsSchemaAsync(cancellationToken);

        var deviceId = NormalizeDeviceId(request.DeviceId);
        if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("DeviceId is required.", nameof(request));

        var now = request.OccurredAtUtc ?? DateTimeOffset.UtcNow;
        var state = await _db.LocationFavoriteStates
            .FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.LocationId == request.LocationId, cancellationToken);
        var previousValue = state?.IsFavorited;

        if (state is null && !request.IsFavorite)
        {
            return;
        }

        if (state is null)
        {
            state = new LocationFavoriteState
            {
                DeviceId = deviceId,
                LocationId = request.LocationId,
                IsFavorited = request.IsFavorite,
                FavoritedAtUtc = request.IsFavorite ? now : null,
                UpdatedAtUtc = now
            };
            await _db.LocationFavoriteStates.AddAsync(state, cancellationToken);
        }
        else if (state.IsFavorited != request.IsFavorite)
        {
            state.IsFavorited = request.IsFavorite;
            state.FavoritedAtUtc = request.IsFavorite ? now : state.FavoritedAtUtc;
            state.UpdatedAtUtc = now;
        }

        if (previousValue == request.IsFavorite)
        {
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var entity = new FavoriteClick
        {
            DeviceId = deviceId,
            LocationId = request.LocationId,
            IsFavorite = request.IsFavorite,
            OccurredAtUtc = now
        };

        await _db.FavoriteClicks.AddAsync(entity, cancellationToken);
        await AddEventAsync(
            deviceId,
            request.IsFavorite ? "favorite_add" : "favorite_remove",
            request.LocationId,
            string.Empty,
            now,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordHeartbeatAsync(AppUsageHeartbeatRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAnalyticsSchemaAsync(cancellationToken);

        var deviceId = NormalizeDeviceId(request.DeviceId);
        if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("DeviceId is required.", nameof(request));

        var path = NormalizePath(request.Path);
        if (IsAdminPath(path)) return;

        var now = request.OccurredAtUtc ?? DateTimeOffset.UtcNow;
        var eventType = VisitEventTypes.Contains(request.EventType) ? request.EventType : "heartbeat";

        await UpsertVisitorDeviceAsync(deviceId, path, request.UserAgent, now, cancellationToken);

        var entity = new AppUsageHeartbeat
        {
            SessionId = request.SessionId?.Trim() ?? string.Empty,
            DeviceId = deviceId,
            OccurredAtUtc = now,
            Platform = request.Platform?.Trim() ?? string.Empty,
            AppVersion = request.AppVersion?.Trim() ?? string.Empty
        };

        await _db.AppUsageHeartbeats.AddAsync(entity, cancellationToken);
        await AddEventAsync(deviceId, eventType, null, path, now, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordEventAsync(AnalyticsEventRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAnalyticsSchemaAsync(cancellationToken);

        var deviceId = NormalizeDeviceId(request.DeviceId);
        if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("DeviceId is required.", nameof(request));

        var eventType = (request.EventType ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("EventType is required.", nameof(request));

        var path = NormalizePath(request.Path);
        if (IsAdminPath(path)) return;

        await AddEventAsync(
            deviceId,
            eventType.Length > 64 ? eventType[..64] : eventType,
            request.LocationId,
            path,
            request.CreatedAtUtc ?? DateTimeOffset.UtcNow,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAnalyticsSchemaAsync(cancellationToken);

        var audioPlays = await _db.AudioPlays.AsNoTracking().LongCountAsync(cancellationToken);
        var favorites = await _db.LocationFavoriteStates.AsNoTracking()
            .LongCountAsync(x => x.IsFavorited, cancellationToken);

        var activeSince = DateTimeOffset.UtcNow.AddMinutes(-5);

        var activeCount = await _db.VisitorDevices.AsNoTracking()
            .CountAsync(x => x.LastSeenAtUtc >= activeSince, cancellationToken);

        var visitors = await _db.VisitorDevices.AsNoTracking()
            .LongCountAsync(cancellationToken);

        var recentVisitors = await _db.VisitorDevices.AsNoTracking()
            .OrderByDescending(x => x.LastSeenAtUtc)
            .Select(x => new VisitorActivityDto
            {
                Id = x.Id,
                DeviceId = x.DeviceId,
                FirstSeenAtUtc = x.FirstSeenAtUtc,
                LastSeenAtUtc = x.LastSeenAtUtc,
                LastPath = x.LastPath,
                LastUserAgent = x.LastUserAgent,
                IsActive = x.LastSeenAtUtc >= activeSince
            })
            .ToListAsync(cancellationToken);

        return new AnalyticsSummaryDto
        {
            CurrentActiveDevices = (int)activeCount,
            TotalAudioPlays = audioPlays,
            TotalFavoritesSaved = favorites,
            TotalVisitorsToday = visitors,
            TotalVisitors = visitors,
            RecentVisitors = recentVisitors
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
            .Where(x => x.IsFavorite && x.OccurredAtUtc.Date >= start && x.OccurredAtUtc.Date <= end)
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

    private static string NormalizeDeviceId(string deviceId)
        => (deviceId ?? string.Empty).Trim();

    private static string NormalizePath(string path)
    {
        var normalized = (path ?? "/").Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "/" : normalized;
    }

    private static bool IsAdminPath(string path)
        => path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase);

    private async Task UpsertVisitorDeviceAsync(
        string deviceId,
        string path,
        string userAgent,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var visitor = await _db.VisitorDevices
            .FirstOrDefaultAsync(x => x.DeviceId == deviceId, cancellationToken);

        if (visitor is null)
        {
            visitor = new VisitorDevice
            {
                DeviceId = deviceId,
                FirstSeenAtUtc = now,
                LastSeenAtUtc = now
            };
            await _db.VisitorDevices.AddAsync(visitor, cancellationToken);
        }

        visitor.LastSeenAtUtc = now;
        visitor.LastPath = path.Length > 1024 ? path[..1024] : path;
        var trimmedUserAgent = (userAgent ?? string.Empty).Trim();
        visitor.LastUserAgent = trimmedUserAgent.Length > 512 ? trimmedUserAgent[..512] : trimmedUserAgent;
    }

    private async Task AddEventAsync(
        string deviceId,
        string eventType,
        int? locationId,
        string path,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
        await _db.AnalyticsEvents.AddAsync(new AnalyticsEvent
        {
            DeviceId = deviceId,
            EventType = eventType,
            LocationId = locationId,
            Path = path.Length > 1024 ? path[..1024] : path,
            CreatedAtUtc = createdAtUtc
        }, cancellationToken);
    }

    private async Task IncrementLocationAudioPlayStatsAsync(IEnumerable<AudioPlay> audioPlays, CancellationToken cancellationToken)
    {
        var groupedCounts = audioPlays
            .GroupBy(item => new { item.LocationId, StatDate = item.OccurredAtUtc.UtcDateTime.Date })
            .Select(group => new
            {
                group.Key.LocationId,
                group.Key.StatDate,
                Count = group.Count()
            })
            .ToArray();

        if (groupedCounts.Length == 0)
        {
            return;
        }

        var locationIds = groupedCounts.Select(item => item.LocationId).Distinct().ToArray();
        var statDates = groupedCounts.Select(item => item.StatDate).Distinct().ToArray();
        var minDate = statDates.Min();
        var maxDate = statDates.Max();

        var existingStats = await _db.LocationStats
            .Where(item => locationIds.Contains(item.LocationId) && item.StatDate >= minDate && item.StatDate <= maxDate)
            .ToListAsync(cancellationToken);

        foreach (var groupedCount in groupedCounts)
        {
            var stat = existingStats.FirstOrDefault(item =>
                item.LocationId == groupedCount.LocationId &&
                item.StatDate.Date == groupedCount.StatDate);

            if (stat is null)
            {
                stat = new LocationStat
                {
                    LocationId = groupedCount.LocationId,
                    StatDate = groupedCount.StatDate,
                    ViewsCount = 0,
                    AudioPlaysCount = 0
                };

                existingStats.Add(stat);
                await _db.LocationStats.AddAsync(stat, cancellationToken);
            }

            stat.AudioPlaysCount += groupedCount.Count;
        }
    }

    private async Task EnsureAnalyticsSchemaAsync(CancellationToken cancellationToken)
    {
        if (_schemaEnsured) return;

        await SchemaLock.WaitAsync(cancellationToken);
        try
        {
            if (_schemaEnsured) return;

            await _db.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS `AudioPlays` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` varchar(128) NOT NULL,
                    `LocationId` int NOT NULL,
                    `AudioId` int NULL,
                    `OccurredAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_AudioPlays` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `FavoriteClicks` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` varchar(128) NOT NULL,
                    `LocationId` int NOT NULL,
                    `IsFavorite` tinyint(1) NOT NULL,
                    `OccurredAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_FavoriteClicks` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `AppUsageHeartbeats` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `SessionId` varchar(128) NOT NULL,
                    `DeviceId` varchar(128) NOT NULL,
                    `OccurredAtUtc` datetime(6) NOT NULL,
                    `Platform` varchar(128) NOT NULL,
                    `AppVersion` varchar(64) NOT NULL,
                    CONSTRAINT `PK_AppUsageHeartbeats` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `VisitorDevices` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` varchar(128) NOT NULL,
                    `FirstSeenAtUtc` datetime(6) NOT NULL,
                    `LastSeenAtUtc` datetime(6) NOT NULL,
                    `LastPath` varchar(1024) NOT NULL,
                    `LastUserAgent` varchar(512) NOT NULL,
                    CONSTRAINT `PK_VisitorDevices` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `AnalyticsEvents` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` varchar(128) NOT NULL,
                    `EventType` varchar(64) NOT NULL,
                    `LocationId` int NULL,
                    `Path` varchar(1024) NOT NULL,
                    `CreatedAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_AnalyticsEvents` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `LocationFavoriteStates` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` varchar(128) NOT NULL,
                    `LocationId` int NOT NULL,
                    `IsFavorited` tinyint(1) NOT NULL,
                    `FavoritedAtUtc` datetime(6) NULL,
                    `UpdatedAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_LocationFavoriteStates` PRIMARY KEY (`Id`)
                );
                """,
                cancellationToken);

            await EnsureIndexAsync("AppUsageHeartbeats", "IX_AppUsageHeartbeats_DeviceId_OccurredAtUtc", "CREATE INDEX `IX_AppUsageHeartbeats_DeviceId_OccurredAtUtc` ON `AppUsageHeartbeats` (`DeviceId`(128), `OccurredAtUtc`);", cancellationToken);
            await EnsureIndexAsync("VisitorDevices", "IX_VisitorDevices_DeviceId", "CREATE UNIQUE INDEX `IX_VisitorDevices_DeviceId` ON `VisitorDevices` (`DeviceId`);", cancellationToken);
            await EnsureIndexAsync("VisitorDevices", "IX_VisitorDevices_LastSeenAtUtc", "CREATE INDEX `IX_VisitorDevices_LastSeenAtUtc` ON `VisitorDevices` (`LastSeenAtUtc`);", cancellationToken);
            await EnsureIndexAsync("AnalyticsEvents", "IX_AnalyticsEvents_DeviceId_CreatedAtUtc", "CREATE INDEX `IX_AnalyticsEvents_DeviceId_CreatedAtUtc` ON `AnalyticsEvents` (`DeviceId`, `CreatedAtUtc`);", cancellationToken);
            await EnsureIndexAsync("AnalyticsEvents", "IX_AnalyticsEvents_EventType_CreatedAtUtc", "CREATE INDEX `IX_AnalyticsEvents_EventType_CreatedAtUtc` ON `AnalyticsEvents` (`EventType`, `CreatedAtUtc`);", cancellationToken);
            await EnsureIndexAsync("LocationFavoriteStates", "IX_LocationFavoriteStates_DeviceId_LocationId", "CREATE UNIQUE INDEX `IX_LocationFavoriteStates_DeviceId_LocationId` ON `LocationFavoriteStates` (`DeviceId`, `LocationId`);", cancellationToken);
            await EnsureIndexAsync("LocationFavoriteStates", "IX_LocationFavoriteStates_IsFavorited", "CREATE INDEX `IX_LocationFavoriteStates_IsFavorited` ON `LocationFavoriteStates` (`IsFavorited`);", cancellationToken);

            _schemaEnsured = true;
        }
        finally
        {
            SchemaLock.Release();
        }
    }

    private async Task EnsureIndexAsync(string tableName, string indexName, string createSql, CancellationToken cancellationToken)
    {
        var indexExists = await _db.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS `Value`
                FROM INFORMATION_SCHEMA.STATISTICS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = {0}
                  AND INDEX_NAME = {1}
                """,
                tableName,
                indexName)
            .SingleAsync(cancellationToken);

        if (indexExists == 0)
        {
            await _db.Database.ExecuteSqlRawAsync(createSql, cancellationToken);
        }
    }
}
