using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public class ProximityService : IProximityService
{
    private readonly IProximityDistanceCalculator _distanceCalculator;
    private readonly ProximityOptions _options;
    private readonly ISettingsService _settingsService;
    private readonly object _syncRoot = new();
    private readonly Dictionary<int, DateTimeOffset> _lastTriggeredAt = [];
    private readonly Dictionary<int, DateTimeOffset> _enteredAt = [];
    private readonly HashSet<int> _currentlyInsideStalls = [];

    public ProximityService(
        IProximityDistanceCalculator distanceCalculator,
        IOptions<ProximityOptions> options,
        ISettingsService settingsService)
    {
        _distanceCalculator = distanceCalculator;
        _options = options.Value;
        _settingsService = settingsService;
    }

    public NearbyStallNotification? ProcessLocationUpdate(
        GeoPoint currentLocation,
        IEnumerable<StallSummary> stalls,
        DateTimeOffset now)
    {
        lock (_syncRoot)
        {
            var triggerRadiusMultiplier = _settingsService.GetSettings().TriggerRadiusMultiplier;
            var eligibleStalls = stalls
                .Where(stall => stall.IsActive && stall.TriggerRadiusMeters > 0)
                .Select(stall => new
                {
                    Stall = stall,
                    EffectiveTriggerRadiusMeters = stall.TriggerRadiusMeters * triggerRadiusMultiplier,
                    DistanceMeters = _distanceCalculator.CalculateMeters(
                        currentLocation,
                        new GeoPoint(stall.Latitude, stall.Longitude))
                })
                .Where(item => item.DistanceMeters <= item.EffectiveTriggerRadiusMeters)
                .ToList();

            var insideStallIds = eligibleStalls
                .Select(item => item.Stall.Id)
                .ToHashSet();

            _currentlyInsideStalls.RemoveWhere(stallId => !insideStallIds.Contains(stallId));

            foreach (var stalledEntryId in _enteredAt.Keys.Except(insideStallIds).ToList())
            {
                _enteredAt.Remove(stalledEntryId);
            }

            foreach (var item in eligibleStalls)
            {
                if (!_enteredAt.ContainsKey(item.Stall.Id))
                {
                    _enteredAt[item.Stall.Id] = now;
                }
            }

            var triggerableStalls = eligibleStalls
                .Where(item => !_currentlyInsideStalls.Contains(item.Stall.Id))
                .Where(item => !_lastTriggeredAt.TryGetValue(item.Stall.Id, out var lastTriggeredAt) ||
                               now - lastTriggeredAt >= _options.TriggerCooldown)
                .Where(item => now - _enteredAt[item.Stall.Id] >= _options.TriggerDebounce)
                .OrderByDescending(item => item.Stall.Priority)
                .ThenBy(item => item.DistanceMeters)
                .ToList();

            if (triggerableStalls.Count == 0)
            {
                return null;
            }

            var selectedStall = triggerableStalls[0];
            _lastTriggeredAt[selectedStall.Stall.Id] = now;

            foreach (var item in eligibleStalls)
            {
                _currentlyInsideStalls.Add(item.Stall.Id);
            }

            return new NearbyStallNotification
            {
                StallId = selectedStall.Stall.Id,
                StallName = selectedStall.Stall.Name,
                Category = selectedStall.Stall.Category,
                DistanceMeters = selectedStall.DistanceMeters,
                TriggerReason = ProximityTriggerReason.EnteredRadius,
                Timestamp = now
            };
        }
    }

    public NearbyStallNotification? EvaluateNearbyStall(
        GeoPoint currentLocation,
        IEnumerable<StallSummary> stalls,
        DateTimeOffset now)
    {
        return ProcessLocationUpdate(currentLocation, stalls, now);
    }

    public void Reset()
    {
        lock (_syncRoot)
        {
            _lastTriggeredAt.Clear();
            _enteredAt.Clear();
            _currentlyInsideStalls.Clear();
        }
    }
}
