using Microsoft.Maui.Devices.Sensors;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public static class StallMapStateHelper
{
    public static NearestStallResult? FindNearestActiveStall(
        GeoPoint userLocation,
        IEnumerable<StallSummary> stalls,
        IProximityDistanceCalculator distanceCalculator)
    {
        return stalls
            .Where(stall => stall.IsActive)
            .Select(stall => new NearestStallResult(
                stall,
                distanceCalculator.CalculateMeters(
                    userLocation,
                    new GeoPoint(stall.Latitude, stall.Longitude))))
            .OrderBy(result => result.DistanceMeters)
            .FirstOrDefault();
    }

    public static bool ShouldCenterOnUserLocation(bool hasCenteredOnUserLocation, Location? userLocation)
    {
        return !hasCenteredOnUserLocation && userLocation is not null;
    }
}

public sealed record NearestStallResult(StallSummary Stall, double DistanceMeters);
