using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public delegate void LocationUpdatedEventHandler(LocationResult result);

public interface ILocationTrackingService : ILocationService
{
    event LocationUpdatedEventHandler? LocationUpdated;

    bool IsTracking { get; }

    Task<LocationResult> StartTrackingAsync(
        LocationTrackingOptions? options = null,
        CancellationToken cancellationToken = default);

    Task StopTrackingAsync();
}
