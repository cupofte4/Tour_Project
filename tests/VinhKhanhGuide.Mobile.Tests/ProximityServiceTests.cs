using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class ProximityServiceTests
{
    private readonly ProximityService _service = new(
        new ProximityDistanceCalculator(),
        Options.Create(new ProximityOptions
        {
            TriggerDebounce = TimeSpan.Zero,
            TriggerCooldown = TimeSpan.FromMinutes(3)
        }),
        new FakeSettingsService());

    [Fact]
    public void ProcessLocationUpdate_Triggers_WhenInsideRadius()
    {
        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var stalls = new[]
        {
            new StallSummary
            {
                Id = 1,
                Name = "Oc Co Lan",
                Category = "Hai san",
                Latitude = 10.76042,
                Longitude = 106.69321,
                TriggerRadiusMeters = 40,
                IsActive = true
            }
        };

        var result = _service.ProcessLocationUpdate(userLocation, stalls, now);

        Assert.NotNull(result);
        Assert.Equal(1, result.StallId);
        Assert.Equal(ProximityTriggerReason.EnteredRadius, result.TriggerReason);
        Assert.Equal(now, result.Timestamp);
    }

    [Fact]
    public void ProcessLocationUpdate_DoesNotTrigger_WhenOutsideRadius()
    {
        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var stalls = new[]
        {
            new StallSummary
            {
                Id = 2,
                Name = "Nuong Gio Dem",
                Category = "Do nuong",
                Latitude = 10.76088,
                Longitude = 106.69402,
                TriggerRadiusMeters = 20,
                IsActive = true
            }
        };

        var result = _service.ProcessLocationUpdate(userLocation, stalls, now);

        Assert.Null(result);
    }

    [Fact]
    public void ProcessLocationUpdate_DoesNotTrigger_InactivePoi()
    {
        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var stalls = new[]
        {
            new StallSummary
            {
                Id = 3,
                Name = "Inactive Poi",
                Category = "Hidden",
                Latitude = 10.76042,
                Longitude = 106.69321,
                TriggerRadiusMeters = 40,
                IsActive = false
            }
        };

        var result = _service.ProcessLocationUpdate(userLocation, stalls, now);

        Assert.Null(result);
    }

    [Fact]
    public void ProcessLocationUpdate_BlocksDuplicateTrigger_DuringCooldown()
    {
        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var stall = new StallSummary
        {
            Id = 1,
            Name = "Oc Co Lan",
            Category = "Hai san",
            Latitude = 10.76042,
            Longitude = 106.69321,
            TriggerRadiusMeters = 40,
            IsActive = true
        };

        var firstResult = _service.ProcessLocationUpdate(userLocation, [stall], now);
        var movedAwayLocation = new GeoPoint(10.77000, 106.70000);
        _service.ProcessLocationUpdate(movedAwayLocation, [stall], now.AddMinutes(1));
        var secondResult = _service.ProcessLocationUpdate(userLocation, [stall], now.AddMinutes(2));

        Assert.NotNull(firstResult);
        Assert.Null(secondResult);
    }

    [Fact]
    public void ProcessLocationUpdate_DebounceSuppressesBoundaryJitter()
    {
        var service = new ProximityService(
            new ProximityDistanceCalculator(),
            Options.Create(new ProximityOptions
            {
                TriggerDebounce = TimeSpan.FromSeconds(10),
                TriggerCooldown = TimeSpan.FromMinutes(3)
            }),
            new FakeSettingsService());

        var now = DateTimeOffset.UtcNow;
        var insideLocation = new GeoPoint(10.76042, 106.69321);
        var outsideLocation = new GeoPoint(10.77000, 106.70000);
        var stall = CreateActiveStall(id: 4, priority: 1, latitude: 10.76042, longitude: 106.69321, radius: 40);

        var firstAttempt = service.ProcessLocationUpdate(insideLocation, [stall], now);
        var jitterOutside = service.ProcessLocationUpdate(outsideLocation, [stall], now.AddSeconds(3));
        var secondAttempt = service.ProcessLocationUpdate(insideLocation, [stall], now.AddSeconds(4));
        var debouncedTrigger = service.ProcessLocationUpdate(insideLocation, [stall], now.AddSeconds(15));

        Assert.Null(firstAttempt);
        Assert.Null(jitterOutside);
        Assert.Null(secondAttempt);
        Assert.NotNull(debouncedTrigger);
    }

    [Fact]
    public void ProcessLocationUpdate_HigherPriorityPoiWins()
    {
        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var lowerPriority = CreateActiveStall(id: 5, priority: 1, latitude: 10.76042, longitude: 106.69321, radius: 60);
        var higherPriority = CreateActiveStall(id: 6, priority: 5, latitude: 10.76045, longitude: 106.69325, radius: 60);

        var result = _service.ProcessLocationUpdate(userLocation, [lowerPriority, higherPriority], now);

        Assert.NotNull(result);
        Assert.Equal(6, result.StallId);
    }

    [Fact]
    public void ProcessLocationUpdate_NearestPoiWins_WhenPriorityTied()
    {
        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var nearest = CreateActiveStall(id: 7, priority: 2, latitude: 10.76042, longitude: 106.69321, radius: 60);
        var farther = CreateActiveStall(id: 8, priority: 2, latitude: 10.76080, longitude: 106.69380, radius: 60);

        var result = _service.ProcessLocationUpdate(userLocation, [farther, nearest], now);

        Assert.NotNull(result);
        Assert.Equal(7, result.StallId);
    }

    [Fact]
    public void ProcessLocationUpdate_StayingInsideSameRadius_DoesNotRetrigger()
    {
        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var stall = CreateActiveStall(id: 9, priority: 1, latitude: 10.76042, longitude: 106.69321, radius: 50);

        var firstResult = _service.ProcessLocationUpdate(userLocation, [stall], now);
        var secondResult = _service.ProcessLocationUpdate(userLocation, [stall], now.AddSeconds(30));

        Assert.NotNull(firstResult);
        Assert.Null(secondResult);
    }

    [Fact]
    public void Reset_AllowsFutureTriggeringAgain()
    {
        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var stall = CreateActiveStall(id: 10, priority: 1, latitude: 10.76042, longitude: 106.69321, radius: 50);

        var firstResult = _service.ProcessLocationUpdate(userLocation, [stall], now);
        _service.Reset();
        var secondResult = _service.ProcessLocationUpdate(userLocation, [stall], now.AddSeconds(1));

        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        Assert.Equal(10, secondResult.StallId);
    }

    [Fact]
    public void ProcessLocationUpdate_UsesTriggerRadiusMultiplier_FromSettings()
    {
        var service = new ProximityService(
            new ProximityDistanceCalculator(),
            Options.Create(new ProximityOptions
            {
                TriggerDebounce = TimeSpan.Zero,
                TriggerCooldown = TimeSpan.FromMinutes(3)
            }),
            new FakeSettingsService
            {
                Settings = new AppSettings
                {
                    TriggerRadiusMultiplier = 2.0d
                }
            });

        var now = DateTimeOffset.UtcNow;
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var stall = CreateActiveStall(id: 11, priority: 1, latitude: 10.76070, longitude: 106.69321, radius: 20);

        var result = service.ProcessLocationUpdate(userLocation, [stall], now);

        Assert.NotNull(result);
        Assert.Equal(11, result.StallId);
    }

    private static StallSummary CreateActiveStall(int id, int priority, double latitude, double longitude, double radius)
    {
        return new StallSummary
        {
            Id = id,
            Name = $"Poi {id}",
            Category = "POI",
            Latitude = latitude,
            Longitude = longitude,
            TriggerRadiusMeters = radius,
            Priority = priority,
            IsActive = true
        };
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public event AppSettingsChangedEventHandler? SettingsChanged;

        public AppSettings Settings { get; set; } = new();

        public AppSettings GetSettings()
        {
            return Settings;
        }

        public void SaveSettings(AppSettings settings)
        {
            Settings = settings;
            SettingsChanged?.Invoke(settings);
        }

        public string GetResolvedApiBaseUrl()
        {
            return "http://localhost:5113/";
        }

        public string GetConfiguredApiBaseUrl()
        {
            return string.Empty;
        }
    }
}
