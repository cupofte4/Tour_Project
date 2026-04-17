namespace VinhKhanhGuide.Mobile.Services;

public sealed class NoopUsageAnalyticsService : IUsageAnalyticsService
{
    public Task OnAppBecameActiveAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnAppWentToBackgroundAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TrackStallViewAsync(int stallId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
