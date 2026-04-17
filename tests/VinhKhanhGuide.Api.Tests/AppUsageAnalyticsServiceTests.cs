using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Application.Analytics;
using VinhKhanhGuide.Infrastructure.Analytics;
using VinhKhanhGuide.Infrastructure.Persistence;

namespace VinhKhanhGuide.Api.Tests;

public sealed class AppUsageAnalyticsServiceTests
{
    [Fact]
    public async Task RecordEventAsync_CreatesSession()
    {
        await using var dbContext = CreateDbContext();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.Parse("2026-04-18T12:00:00Z"));
        var service = CreateService(dbContext, timeProvider);

        await service.RecordEventAsync(new AppUsageEventIngestRequest
        {
            SessionId = "session-1",
            AnonymousClientId = "anon-1",
            EventType = AppUsageEventType.AppOpen,
            Platform = "ios",
            AppVersion = "1.0.0"
        });

        var session = await dbContext.AppUsageSessions.SingleAsync();
        Assert.Equal("session-1", session.SessionId);
        Assert.Equal("anon-1", session.AnonymousClientId);
        Assert.Equal(AppUsageEventType.AppOpen, session.LastEventType);
    }

    [Fact]
    public async Task RecordHeartbeatAsync_UpdatesLastSeenAtUtc()
    {
        await using var dbContext = CreateDbContext();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.Parse("2026-04-18T12:00:00Z"));
        var service = CreateService(dbContext, timeProvider);

        await service.RecordEventAsync(new AppUsageEventIngestRequest
        {
            SessionId = "session-2",
            AnonymousClientId = "anon-2",
            EventType = AppUsageEventType.AppOpen
        });

        timeProvider.SetUtcNow(DateTimeOffset.Parse("2026-04-18T12:02:00Z"));

        await service.RecordHeartbeatAsync(new AppUsageHeartbeatRequest
        {
            SessionId = "session-2",
            AnonymousClientId = "anon-2"
        });

        var session = await dbContext.AppUsageSessions.SingleAsync();
        Assert.Equal(DateTimeOffset.Parse("2026-04-18T12:02:00Z"), session.LastSeenAtUtc);
        Assert.Equal(AppUsageEventType.Heartbeat, session.LastEventType);
    }

    [Fact]
    public async Task GetActiveUsersAsync_CountsOnlyRecentSessions()
    {
        await using var dbContext = CreateDbContext();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.Parse("2026-04-18T12:00:00Z"));
        var service = CreateService(dbContext, timeProvider);

        await service.RecordEventAsync(new AppUsageEventIngestRequest
        {
            SessionId = "session-active",
            AnonymousClientId = "anon-active",
            EventType = AppUsageEventType.AppOpen,
            OccurredAtUtc = DateTimeOffset.Parse("2026-04-18T11:58:30Z")
        });
        await service.RecordEventAsync(new AppUsageEventIngestRequest
        {
            SessionId = "session-old",
            AnonymousClientId = "anon-old",
            EventType = AppUsageEventType.AppOpen,
            OccurredAtUtc = DateTimeOffset.Parse("2026-04-18T11:55:00Z")
        });

        var metric = await service.GetActiveUsersAsync();

        Assert.Equal(1, metric.ActiveUsers);
        Assert.Equal(3, metric.WindowMinutes);
        Assert.Equal(DateTimeOffset.Parse("2026-04-18T12:00:00Z"), metric.LastUpdatedUtc);
    }

    private static AppUsageAnalyticsService CreateService(AppDbContext dbContext, TimeProvider timeProvider)
    {
        return new AppUsageAnalyticsService(
            dbContext,
            Options.Create(new AppUsageAnalyticsOptions
            {
                Enabled = true,
                ActiveUserWindowMinutes = 3
            }),
            timeProvider);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AppDbContext(options);
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }

        public void SetUtcNow(DateTimeOffset value)
        {
            _utcNow = value;
        }
    }
}
