namespace VinhKhanhGuide.Mobile.Services;

public interface IUsageAnalyticsService
{
    Task OnAppBecameActiveAsync(CancellationToken cancellationToken = default);

    Task OnAppWentToBackgroundAsync(CancellationToken cancellationToken = default);

    Task TrackStallViewAsync(int stallId, CancellationToken cancellationToken = default);
}
