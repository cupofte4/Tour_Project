using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IProximityService
{
    NearbyStallNotification? EvaluateNearbyStall(
        GeoPoint currentLocation,
        IEnumerable<StallSummary> stalls,
        DateTimeOffset now);
}
