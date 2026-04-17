using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public sealed class UsageAnalyticsServiceTests
{
    [Fact]
    public async Task OnAppBecameActiveAsync_AndTrackStallViewAsync_ReuseSessionIdAndAnonymousClientId()
    {
        var analyticsApiClient = new RecordingAnalyticsApiClient();
        var settingsStorage = new InMemorySettingsStorage();
        var service = CreateService(analyticsApiClient, settingsStorage, heartbeatIntervalSeconds: 60);

        await service.OnAppBecameActiveAsync();
        await service.TrackStallViewAsync(12);

        Assert.Equal(2, analyticsApiClient.Events.Count);
        Assert.Equal(AppUsageEventTypes.AppOpen, analyticsApiClient.Events[0].EventType);
        Assert.Equal(AppUsageEventTypes.StallView, analyticsApiClient.Events[1].EventType);
        Assert.Equal(analyticsApiClient.Events[0].SessionId, analyticsApiClient.Events[1].SessionId);
        Assert.Equal(analyticsApiClient.Events[0].AnonymousClientId, analyticsApiClient.Events[1].AnonymousClientId);
        Assert.Equal(settingsStorage.StoredClientId, analyticsApiClient.Events[0].AnonymousClientId);
    }

    [Fact]
    public async Task OnAppWentToBackgroundAsync_StopsHeartbeatScheduling()
    {
        var analyticsApiClient = new RecordingAnalyticsApiClient();
        var service = CreateService(analyticsApiClient, new InMemorySettingsStorage(), heartbeatIntervalSeconds: 1);

        await service.OnAppBecameActiveAsync();
        await Task.Delay(1200);
        await service.OnAppWentToBackgroundAsync();
        var heartbeatCountAfterStop = analyticsApiClient.Heartbeats.Count;

        await Task.Delay(1200);

        Assert.True(heartbeatCountAfterStop >= 1);
        Assert.Equal(heartbeatCountAfterStop, analyticsApiClient.Heartbeats.Count);
    }

    [Fact]
    public async Task OnAppBecameActiveAsync_DoesNotCreateDuplicateAppOpenWhileAlreadyActive()
    {
        var analyticsApiClient = new RecordingAnalyticsApiClient();
        var service = CreateService(analyticsApiClient, new InMemorySettingsStorage(), heartbeatIntervalSeconds: 60);

        await service.OnAppBecameActiveAsync();
        await service.OnAppBecameActiveAsync();

        Assert.Single(analyticsApiClient.Events);
        Assert.Equal(AppUsageEventTypes.AppOpen, analyticsApiClient.Events[0].EventType);
    }

    private static UsageAnalyticsService CreateService(
        IAnalyticsApiClient analyticsApiClient,
        ISettingsStorage settingsStorage,
        int heartbeatIntervalSeconds)
    {
        return new UsageAnalyticsService(
            analyticsApiClient,
            new TestAppUsageEnvironmentInfo(),
            settingsStorage,
            Options.Create(new AppUsageAnalyticsOptions
            {
                Enabled = true,
                HeartbeatIntervalSeconds = heartbeatIntervalSeconds
            }),
            TimeProvider.System);
    }

    private sealed class RecordingAnalyticsApiClient : IAnalyticsApiClient
    {
        public List<AppUsageEventRequest> Events { get; } = [];

        public List<AppUsageHeartbeatRequest> Heartbeats { get; } = [];

        public Task PostEventAsync(AppUsageEventRequest request, CancellationToken cancellationToken = default)
        {
            Events.Add(request);
            return Task.CompletedTask;
        }

        public Task PostHeartbeatAsync(AppUsageHeartbeatRequest request, CancellationToken cancellationToken = default)
        {
            Heartbeats.Add(request);
            return Task.CompletedTask;
        }
    }

    private sealed class TestAppUsageEnvironmentInfo : IAppUsageEnvironmentInfo
    {
        public string Platform => "ios";

        public string AppVersion => "1.0.0";
    }

    private sealed class InMemorySettingsStorage : ISettingsStorage
    {
        private readonly Dictionary<string, string> _strings = new(StringComparer.Ordinal);

        public string StoredClientId => GetString("analytics.client.id", string.Empty);

        public bool GetBool(string key, bool defaultValue)
        {
            return defaultValue;
        }

        public double GetDouble(string key, double defaultValue)
        {
            return defaultValue;
        }

        public string GetString(string key, string defaultValue)
        {
            return _strings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void SetBool(string key, bool value)
        {
        }

        public void SetDouble(string key, double value)
        {
        }

        public void SetString(string key, string value)
        {
            _strings[key] = value;
        }
    }
}
