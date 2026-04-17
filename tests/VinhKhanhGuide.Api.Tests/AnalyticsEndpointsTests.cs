using System.Net;
using System.Net.Http.Json;
using VinhKhanhGuide.Application.Analytics;

namespace VinhKhanhGuide.Api.Tests;

public sealed class AnalyticsEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AnalyticsEndpointsTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostEvent_ThenGetActiveUsers_ReturnsCompactMetricPayload()
    {
        var sessionId = Guid.NewGuid().ToString("N");

        var postResponse = await _client.PostAsJsonAsync(
            "/api/analytics/events",
            new AppUsageEventIngestRequest
            {
                SessionId = sessionId,
                AnonymousClientId = "anon-1",
                EventType = AppUsageEventType.AppOpen,
                OccurredAtUtc = DateTimeOffset.UtcNow,
                Platform = "ios",
                AppVersion = "1.0.0"
            });
        var metric = await _client.GetFromJsonAsync<ActiveUsersMetricDto>("/api/admin/metrics/active-users");

        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);
        Assert.NotNull(metric);
        Assert.Equal(1, metric!.ActiveUsers);
        Assert.Equal(3, metric.WindowMinutes);
    }

    [Fact]
    public async Task PostHeartbeat_KeepsExistingSessionActive()
    {
        var sessionId = Guid.NewGuid().ToString("N");

        await _client.PostAsJsonAsync(
            "/api/analytics/events",
            new AppUsageEventIngestRequest
            {
                SessionId = sessionId,
                AnonymousClientId = "anon-2",
                EventType = AppUsageEventType.AppOpen,
                Platform = "ios",
                AppVersion = "1.0.0"
            });
        var heartbeatResponse = await _client.PostAsJsonAsync(
            "/api/analytics/heartbeat",
            new AppUsageHeartbeatRequest
            {
                SessionId = sessionId,
                AnonymousClientId = "anon-2",
                Platform = "ios",
                AppVersion = "1.0.0"
            });
        var metric = await _client.GetFromJsonAsync<ActiveUsersMetricDto>("/api/admin/metrics/active-users");

        Assert.Equal(HttpStatusCode.Accepted, heartbeatResponse.StatusCode);
        Assert.NotNull(metric);
        Assert.Equal(1, metric!.ActiveUsers);
    }

    [Fact]
    public async Task PostEvent_ReturnsBadRequest_WhenEventTypeIsInvalid()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/analytics/events",
            new AppUsageEventIngestRequest
            {
                SessionId = Guid.NewGuid().ToString("N"),
                AnonymousClientId = "anon-3",
                EventType = "invalid",
                Platform = "ios",
                AppVersion = "1.0.0"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
