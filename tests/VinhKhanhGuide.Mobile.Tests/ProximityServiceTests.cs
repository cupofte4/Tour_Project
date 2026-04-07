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
            TriggerCooldown = TimeSpan.FromMinutes(3)
        }));

    [Fact]
    public void EvaluateNearbyStall_Triggers_WhenInsideRadius()
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
                TriggerRadiusMeters = 40
            }
        };

        var result = _service.EvaluateNearbyStall(userLocation, stalls, now);

        Assert.NotNull(result);
        Assert.Equal(1, result.StallId);
    }

    [Fact]
    public void EvaluateNearbyStall_DoesNotTrigger_WhenOutsideRadius()
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
                TriggerRadiusMeters = 20
            }
        };

        var result = _service.EvaluateNearbyStall(userLocation, stalls, now);

        Assert.Null(result);
    }

    [Fact]
    public void EvaluateNearbyStall_BlocksDuplicateTrigger_DuringCooldown()
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
            TriggerRadiusMeters = 40
        };

        var firstResult = _service.EvaluateNearbyStall(userLocation, [stall], now);
        var movedAwayLocation = new GeoPoint(10.77000, 106.70000);
        _service.EvaluateNearbyStall(movedAwayLocation, [stall], now.AddMinutes(1));
        var secondResult = _service.EvaluateNearbyStall(userLocation, [stall], now.AddMinutes(2));

        Assert.NotNull(firstResult);
        Assert.Null(secondResult);
    }
}
