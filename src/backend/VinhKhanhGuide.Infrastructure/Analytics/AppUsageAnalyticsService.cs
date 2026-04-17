using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Application.Analytics;
using VinhKhanhGuide.Domain.Entities;
using VinhKhanhGuide.Infrastructure.Persistence;

namespace VinhKhanhGuide.Infrastructure.Analytics;

public sealed class AppUsageAnalyticsService(
    AppDbContext dbContext,
    IOptions<AppUsageAnalyticsOptions> options,
    TimeProvider timeProvider) : IAppUsageAnalyticsService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly AppUsageAnalyticsOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    public Task RecordEventAsync(AppUsageEventIngestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            throw new ArgumentException("SessionId is required.", nameof(request));
        }

        if (!AppUsageEventType.IsValid(request.EventType))
        {
            throw new ArgumentException("EventType is invalid.", nameof(request));
        }

        return UpsertSessionAsync(
            request.SessionId,
            request.AnonymousClientId,
            request.EventType,
            request.OccurredAtUtc,
            request.Platform,
            request.AppVersion,
            cancellationToken);
    }

    public Task RecordHeartbeatAsync(AppUsageHeartbeatRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            throw new ArgumentException("SessionId is required.", nameof(request));
        }

        return UpsertSessionAsync(
            request.SessionId,
            request.AnonymousClientId,
            AppUsageEventType.Heartbeat,
            request.OccurredAtUtc,
            request.Platform,
            request.AppVersion,
            cancellationToken);
    }

    public async Task<ActiveUsersMetricDto> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var activeThreshold = now.AddMinutes(-_options.ActiveUserWindowMinutes);

        var activeUsers = await _dbContext.AppUsageSessions
            .AsNoTracking()
            .Where(session =>
                session.LastSeenAtUtc >= activeThreshold &&
                session.LastEventType != AppUsageEventType.SessionStopped)
            .CountAsync(cancellationToken);

        return new ActiveUsersMetricDto
        {
            ActiveUsers = activeUsers,
            WindowMinutes = _options.ActiveUserWindowMinutes,
            LastUpdatedUtc = now
        };
    }

    private async Task UpsertSessionAsync(
        string sessionId,
        string anonymousClientId,
        string eventType,
        DateTimeOffset? occurredAtUtc,
        string platform,
        string appVersion,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var normalizedSessionId = sessionId.Trim();
        var now = _timeProvider.GetUtcNow();
        var activityTime = NormalizeActivityTime(occurredAtUtc, now);
        var session = await _dbContext.AppUsageSessions.SingleOrDefaultAsync(
            existing => existing.SessionId == normalizedSessionId,
            cancellationToken);

        if (session is null)
        {
            session = new AppUsageSession
            {
                SessionId = normalizedSessionId,
                AnonymousClientId = NormalizeAnonymousClientId(anonymousClientId, normalizedSessionId),
                FirstSeenAtUtc = activityTime,
                LastSeenAtUtc = activityTime,
                LastEventType = eventType,
                Platform = NormalizePlatform(platform),
                AppVersion = NormalizeAppVersion(appVersion)
            };

            await _dbContext.AppUsageSessions.AddAsync(session, cancellationToken);
        }
        else
        {
            session.AnonymousClientId = NormalizeAnonymousClientId(anonymousClientId, session.AnonymousClientId);
            session.LastSeenAtUtc = activityTime > session.LastSeenAtUtc ? activityTime : session.LastSeenAtUtc;
            session.LastEventType = eventType;
            session.Platform = NormalizePlatform(platform);
            session.AppVersion = NormalizeAppVersion(appVersion);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTimeOffset NormalizeActivityTime(DateTimeOffset? occurredAtUtc, DateTimeOffset fallbackUtc)
    {
        return occurredAtUtc ?? fallbackUtc;
    }

    private static string NormalizeAnonymousClientId(string? anonymousClientId, string fallback)
    {
        return string.IsNullOrWhiteSpace(anonymousClientId)
            ? fallback
            : anonymousClientId.Trim();
    }

    private static string NormalizePlatform(string? platform)
    {
        return string.IsNullOrWhiteSpace(platform)
            ? "unknown"
            : platform.Trim().ToLowerInvariant();
    }

    private static string NormalizeAppVersion(string? appVersion)
    {
        return string.IsNullOrWhiteSpace(appVersion)
            ? string.Empty
            : appVersion.Trim();
    }
}
