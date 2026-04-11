using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public static class LocationTrackingStatusMapper
{
    private static readonly TimeSpan MinimumUpdateInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MinimumRequestTimeout = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan DefaultMaxLastKnownLocationAge = TimeSpan.FromMinutes(2);

    public static LocationTrackingOptions NormalizeOptions(LocationTrackingOptions? options)
    {
        var source = options ?? new LocationTrackingOptions();
        var updateInterval = source.UpdateInterval < MinimumUpdateInterval
            ? MinimumUpdateInterval
            : source.UpdateInterval;
        var requestTimeout = source.RequestTimeout < MinimumRequestTimeout
            ? MinimumRequestTimeout
            : source.RequestTimeout;

        if (requestTimeout < updateInterval)
        {
            requestTimeout = updateInterval;
        }

        return new LocationTrackingOptions
        {
            Accuracy = source.Accuracy,
            UpdateInterval = updateInterval,
            RequestTimeout = requestTimeout,
            UseLastKnownLocationFirst = source.UseLastKnownLocationFirst,
            EnableBackgroundUpdates = source.EnableBackgroundUpdates
        };
    }

    public static LocationResult FromPermissionStatus(PermissionStatus status)
    {
        return status switch
        {
            PermissionStatus.Granted or PermissionStatus.Limited => new LocationResult
            {
                Status = LocationTrackingStatus.Ready
            },
            PermissionStatus.Restricted => new LocationResult
            {
                Status = LocationTrackingStatus.PermissionRestricted,
                Message = "Location access is restricted on this device.",
                PermissionDenied = true
            },
            PermissionStatus.Disabled => new LocationResult
            {
                Status = LocationTrackingStatus.LocationServicesDisabled,
                Message = "Location services are turned off for this device.",
                PermissionDenied = true
            },
            _ => new LocationResult
            {
                Status = LocationTrackingStatus.PermissionDenied,
                Message = "Location permission was denied. Showing POI pins only.",
                PermissionDenied = true
            }
        };
    }

    public static LocationResult CreateUnavailableResult(LocationTrackingStatus status, string message)
    {
        return new LocationResult
        {
            Status = status,
            Message = message,
            PermissionDenied = status is LocationTrackingStatus.PermissionDenied
                or LocationTrackingStatus.PermissionRestricted
                or LocationTrackingStatus.LocationServicesDisabled
        };
    }

    public static bool IsUsableLastKnownLocation(
        Location? location,
        DateTimeOffset utcNow,
        TimeSpan? maxAge = null)
    {
        if (location is null || location.Timestamp == default)
        {
            return false;
        }

        var allowedAge = maxAge ?? DefaultMaxLastKnownLocationAge;
        return utcNow - location.Timestamp.ToUniversalTime() <= allowedAge;
    }
}
