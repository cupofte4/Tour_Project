using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class UsageAnalyticsService : IUsageAnalyticsService, IDisposable
{
    private const string AnonymousClientIdKey = "analytics.client.id";

    private readonly IAnalyticsApiClient _analyticsApiClient;
    private readonly IAppUsageEnvironmentInfo _environmentInfo;
    private readonly ISettingsStorage _settingsStorage;
    private readonly AppUsageAnalyticsOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private readonly string _sessionId = Guid.NewGuid().ToString("N");
    private readonly string _anonymousClientId;

    private CancellationTokenSource? _heartbeatCancellationTokenSource;
    private Task? _heartbeatTask;
    private bool _isActive;

    public UsageAnalyticsService(
        IAnalyticsApiClient analyticsApiClient,
        IAppUsageEnvironmentInfo environmentInfo,
        ISettingsStorage settingsStorage,
        Microsoft.Extensions.Options.IOptions<AppUsageAnalyticsOptions> options,
        TimeProvider timeProvider)
    {
        _analyticsApiClient = analyticsApiClient;
        _environmentInfo = environmentInfo;
        _settingsStorage = settingsStorage;
        _options = options.Value;
        _timeProvider = timeProvider;
        _anonymousClientId = GetOrCreateAnonymousClientId();
    }

    public async Task OnAppBecameActiveAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await _stateLock.WaitAsync(cancellationToken);

        try
        {
            if (_isActive)
            {
                return;
            }

            _isActive = true;
            _heartbeatCancellationTokenSource = new CancellationTokenSource();
            _heartbeatTask = RunHeartbeatLoopAsync(_heartbeatCancellationTokenSource.Token);
        }
        finally
        {
            _stateLock.Release();
        }

        await SendEventSafelyAsync(AppUsageEventTypes.AppOpen, cancellationToken);
    }

    public async Task OnAppWentToBackgroundAsync(CancellationToken cancellationToken = default)
    {
        CancellationTokenSource? heartbeatCancellationTokenSource;
        Task? heartbeatTask;

        await _stateLock.WaitAsync(cancellationToken);

        try
        {
            if (!_isActive)
            {
                return;
            }

            _isActive = false;
            heartbeatCancellationTokenSource = _heartbeatCancellationTokenSource;
            heartbeatTask = _heartbeatTask;
            _heartbeatCancellationTokenSource = null;
            _heartbeatTask = null;
        }
        finally
        {
            _stateLock.Release();
        }

        heartbeatCancellationTokenSource?.Cancel();

        if (heartbeatTask is not null)
        {
            try
            {
                await heartbeatTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        heartbeatCancellationTokenSource?.Dispose();
    }

    public Task TrackStallViewAsync(int stallId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || stallId <= 0)
        {
            return Task.CompletedTask;
        }

        return SendEventSafelyAsync(AppUsageEventTypes.StallView, cancellationToken);
    }

    public void Dispose()
    {
        _heartbeatCancellationTokenSource?.Cancel();
        _heartbeatCancellationTokenSource?.Dispose();
        _stateLock.Dispose();
    }

    private async Task RunHeartbeatLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(Math.Max(1, _options.HeartbeatIntervalSeconds)),
            _timeProvider);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await SendHeartbeatSafelyAsync(cancellationToken);
        }
    }

    private async Task SendEventSafelyAsync(string eventType, CancellationToken cancellationToken)
    {
        try
        {
            await _analyticsApiClient.PostEventAsync(
                new AppUsageEventRequest
                {
                    SessionId = _sessionId,
                    AnonymousClientId = _anonymousClientId,
                    EventType = eventType,
                    OccurredAtUtc = _timeProvider.GetUtcNow(),
                    Platform = _environmentInfo.Platform,
                    AppVersion = _environmentInfo.AppVersion
                },
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
        }
        catch (HttpRequestException)
        {
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task SendHeartbeatSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _analyticsApiClient.PostHeartbeatAsync(
                new AppUsageHeartbeatRequest
                {
                    SessionId = _sessionId,
                    AnonymousClientId = _anonymousClientId,
                    OccurredAtUtc = _timeProvider.GetUtcNow(),
                    Platform = _environmentInfo.Platform,
                    AppVersion = _environmentInfo.AppVersion
                },
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
        }
        catch (HttpRequestException)
        {
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
        }
    }

    private string GetOrCreateAnonymousClientId()
    {
        var existingValue = _settingsStorage.GetString(AnonymousClientIdKey, string.Empty).Trim();

        if (!string.IsNullOrWhiteSpace(existingValue))
        {
            return existingValue;
        }

        var newValue = Guid.NewGuid().ToString("N");
        _settingsStorage.SetString(AnonymousClientIdKey, newValue);
        return newValue;
    }
}
