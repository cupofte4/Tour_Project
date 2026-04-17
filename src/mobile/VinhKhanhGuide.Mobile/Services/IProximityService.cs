using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IProximityService
{
    NearbyStallNotification? ProcessLocationUpdate(
        GeoPoint currentLocation,
        IEnumerable<StallSummary> stalls,
        DateTimeOffset now);

    NearbyStallNotification? EvaluateNearbyStall(
        GeoPoint currentLocation,
        IEnumerable<StallSummary> stalls,
        DateTimeOffset now);

    void Reset();
}
