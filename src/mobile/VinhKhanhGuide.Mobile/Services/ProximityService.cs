using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public class ProximityService : IProximityService
{
    private readonly IProximityDistanceCalculator _distanceCalculator;
    private readonly ProximityOptions _options;
    private readonly Dictionary<int, DateTimeOffset> _lastTriggeredAt = [];
    private readonly HashSet<int> _currentlyInsideStalls = [];

    public ProximityService(
        IProximityDistanceCalculator distanceCalculator,
        IOptions<ProximityOptions> options)
    {
        _distanceCalculator = distanceCalculator;
        _options = options.Value;
    }

    public NearbyStallNotification? EvaluateNearbyStall(
        GeoPoint currentLocation,
        IEnumerable<StallSummary> stalls,
        DateTimeOffset now)
    {
        var insideStalls = stalls
            .Select(stall => new
            {
                Stall = stall,
                DistanceMeters = _distanceCalculator.CalculateMeters(
                    currentLocation,
                    new GeoPoint(stall.Latitude, stall.Longitude))
            })
            .Where(item => item.DistanceMeters <= item.Stall.TriggerRadiusMeters)
            .OrderBy(item => item.DistanceMeters)
            .ToList();

        var insideStallIds = insideStalls
            .Select(item => item.Stall.Id)
            .ToHashSet();

        _currentlyInsideStalls.RemoveWhere(stallId => !insideStallIds.Contains(stallId));

        foreach (var item in insideStalls)
        {
            if (_currentlyInsideStalls.Contains(item.Stall.Id))
            {
                continue;
            }

            if (_lastTriggeredAt.TryGetValue(item.Stall.Id, out var lastTriggeredAt) &&
                now - lastTriggeredAt < _options.TriggerCooldown)
            {
                _currentlyInsideStalls.Add(item.Stall.Id);
                continue;
            }

            _lastTriggeredAt[item.Stall.Id] = now;
            _currentlyInsideStalls.Add(item.Stall.Id);

            return new NearbyStallNotification
            {
                StallId = item.Stall.Id,
                StallName = item.Stall.Name,
                Category = item.Stall.Category,
                DistanceMeters = item.DistanceMeters
            };
        }

        return null;
    }
}
