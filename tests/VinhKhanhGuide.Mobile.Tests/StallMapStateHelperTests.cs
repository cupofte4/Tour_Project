using Microsoft.Maui.Devices.Sensors;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class StallMapStateHelperTests
{
    private readonly ProximityDistanceCalculator _distanceCalculator = new();

    [Fact]
    public void FindNearestActiveStall_ReturnsNearestActivePoi()
    {
        var userLocation = new GeoPoint(10.76042, 106.69321);
        var stalls = new[]
        {
            new StallSummary
            {
                Id = 1,
                Name = "Inactive",
                Latitude = 10.76042,
                Longitude = 106.69321,
                IsActive = false
            },
            new StallSummary
            {
                Id = 2,
                Name = "Nearest",
                Latitude = 10.76043,
                Longitude = 106.69322,
                IsActive = true
            },
            new StallSummary
            {
                Id = 3,
                Name = "Farther",
                Latitude = 10.76150,
                Longitude = 106.69450,
                IsActive = true
            }
        };

        var result = StallMapStateHelper.FindNearestActiveStall(userLocation, stalls, _distanceCalculator);

        Assert.NotNull(result);
        Assert.Equal(2, result.Stall.Id);
    }

    [Fact]
    public void ShouldCenterOnUserLocation_OnlyReturnsTrueBeforeFirstUserCenter()
    {
        Assert.True(StallMapStateHelper.ShouldCenterOnUserLocation(false, new Location(10.76042, 106.69321)));
        Assert.False(StallMapStateHelper.ShouldCenterOnUserLocation(true, new Location(10.76042, 106.69321)));
        Assert.False(StallMapStateHelper.ShouldCenterOnUserLocation(false, null));
    }
}
