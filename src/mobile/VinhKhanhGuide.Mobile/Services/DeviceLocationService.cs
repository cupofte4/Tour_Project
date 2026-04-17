using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public class DeviceLocationService(
    IOptions<LocationTrackingOptions> defaultOptions,
    ISettingsService settingsService) : ILocationTrackingService
{
    private static readonly GeoPoint DefaultFallbackLocation = new(10.76042, 106.69321);
    private readonly object _syncRoot = new();
    private CancellationTokenSource? _trackingCts;
    private Task? _trackingTask;
    private LocationTrackingOptions _currentOptions = LocationTrackingStatusMapper.NormalizeOptions(defaultOptions.Value);

    public event LocationUpdatedEventHandler? LocationUpdated;

    public bool IsTracking
    {
        get
        {
            lock (_syncRoot)
            {
                return _trackingCts is not null;
            }
        }
    }

    public async Task<LocationResult> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        if (!settingsService.GetSettings().IsGpsTrackingEnabled)
        {
            return LocationTrackingStatusMapper.CreateUnavailableResult(
                LocationTrackingStatus.LocationServicesDisabled,
                "GPS tracking is turned off in Settings. Nearby prompts are paused.");
        }

        var currentOptions = _currentOptions;
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (permissionStatus is not PermissionStatus.Granted and not PermissionStatus.Limited)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (permissionStatus is not PermissionStatus.Granted and not PermissionStatus.Limited)
        {
            return CreateFallbackLocationResult(
                LocationTrackingStatusMapper.FromPermissionStatus(permissionStatus).Status,
                "Location permission is unavailable. Using the default demo location near Vinh Khanh.");
        }

        try
        {
            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(currentOptions.Accuracy, currentOptions.RequestTimeout),
                cancellationToken);

            if (location is null && currentOptions.UseLastKnownLocationFirst)
            {
                var lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();

                if (LocationTrackingStatusMapper.IsUsableLastKnownLocation(
                    lastKnownLocation,
                    DateTimeOffset.UtcNow))
                {
                    location = lastKnownLocation;
                }
            }

            if (location is null)
            {
                return CreateFallbackLocationResult(
                    LocationTrackingStatus.TemporarilyUnavailable,
                    "Current location is unavailable. Using the default demo location near Vinh Khanh.");
            }

            return new LocationResult
            {
                CurrentLocation = new GeoPoint(location.Latitude, location.Longitude),
                Status = LocationTrackingStatus.Ready
            };
        }
        catch (FeatureNotEnabledException)
        {
            return CreateFallbackLocationResult(
                LocationTrackingStatus.LocationServicesDisabled,
                "Location services are turned off for this device. Using the default demo location near Vinh Khanh.");
        }
        catch (FeatureNotSupportedException)
        {
            return CreateFallbackLocationResult(
                LocationTrackingStatus.Unsupported,
                "Location services are unavailable on this device. Using the default demo location near Vinh Khanh.");
        }
        catch (PermissionException)
        {
            return CreateFallbackLocationResult(
                LocationTrackingStatus.PermissionDenied,
                "Location permission was denied. Using the default demo location near Vinh Khanh.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return CreateFallbackLocationResult(
                LocationTrackingStatus.TemporarilyUnavailable,
                "Current location is temporarily unavailable. Using the default demo location near Vinh Khanh.");
        }
    }

    public async Task<LocationResult> StartTrackingAsync(
        LocationTrackingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var alreadyTracking = false;
        _currentOptions = LocationTrackingStatusMapper.NormalizeOptions(options ?? defaultOptions.Value);

        if (!settingsService.GetSettings().IsGpsTrackingEnabled)
        {
            return LocationTrackingStatusMapper.CreateUnavailableResult(
                LocationTrackingStatus.LocationServicesDisabled,
                "GPS tracking is turned off in Settings. Nearby prompts are paused.");
        }

        lock (_syncRoot)
        {
            if (_trackingCts is not null)
            {
                alreadyTracking = true;
            }
            else
            {
                _trackingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _trackingTask = RunTrackingLoopAsync(_trackingCts.Token);
            }
        }

        if (alreadyTracking)
        {
            return await GetCurrentLocationAsync(cancellationToken);
        }

        try
        {
            return await RefreshAndPublishAsync(cancellationToken);
        }
        catch
        {
            await StopTrackingAsync();
            throw;
        }
    }

    public async Task StopTrackingAsync()
    {
        CancellationTokenSource? trackingCts;
        Task? trackingTask;

        lock (_syncRoot)
        {
            trackingCts = _trackingCts;
            trackingTask = _trackingTask;
            _trackingCts = null;
            _trackingTask = null;
        }

        if (trackingCts is null)
        {
            return;
        }

        trackingCts.Cancel();

        if (trackingTask is not null)
        {
            try
            {
                await trackingTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        trackingCts.Dispose();
    }

    private async Task RunTrackingLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_currentOptions.UpdateInterval, cancellationToken);
            await RefreshAndPublishAsync(cancellationToken);
        }
    }

    private async Task<LocationResult> RefreshAndPublishAsync(CancellationToken cancellationToken)
    {
        var result = await GetCurrentLocationAsync(cancellationToken);
        await PublishUpdateAsync(result);
        return result;
    }

    private async Task PublishUpdateAsync(LocationResult result)
    {
        if (LocationUpdated is null)
        {
            return;
        }

        if (MainThread.IsMainThread)
        {
            LocationUpdated.Invoke(result);
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(() => LocationUpdated.Invoke(result));
    }

    private static LocationResult CreateFallbackLocationResult(LocationTrackingStatus status, string message)
    {
        return new LocationResult
        {
            CurrentLocation = DefaultFallbackLocation,
            Status = status,
            Message = $"{message} (10.76042, 106.69321)",
            PermissionDenied = status is LocationTrackingStatus.PermissionDenied
                or LocationTrackingStatus.PermissionRestricted
                or LocationTrackingStatus.LocationServicesDisabled
        };
    }
}
