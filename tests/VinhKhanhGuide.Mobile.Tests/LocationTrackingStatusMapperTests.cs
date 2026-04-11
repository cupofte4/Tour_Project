using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class LocationTrackingStatusMapperTests
{
    [Fact]
    public void NormalizeOptions_EnforcesMinimums()
    {
        var options = new LocationTrackingOptions
        {
            Accuracy = GeolocationAccuracy.Best,
            UpdateInterval = TimeSpan.FromSeconds(1),
            RequestTimeout = TimeSpan.FromSeconds(2)
        };

        var normalized = LocationTrackingStatusMapper.NormalizeOptions(options);

        Assert.Equal(GeolocationAccuracy.Best, normalized.Accuracy);
        Assert.Equal(TimeSpan.FromSeconds(5), normalized.UpdateInterval);
        Assert.Equal(TimeSpan.FromSeconds(5), normalized.RequestTimeout);
    }

    [Theory]
    [InlineData(PermissionStatus.Denied, LocationTrackingStatus.PermissionDenied, true)]
    [InlineData(PermissionStatus.Restricted, LocationTrackingStatus.PermissionRestricted, true)]
    [InlineData(PermissionStatus.Disabled, LocationTrackingStatus.LocationServicesDisabled, true)]
    [InlineData(PermissionStatus.Granted, LocationTrackingStatus.Ready, false)]
    public void FromPermissionStatus_MapsExpectedState(
        PermissionStatus permissionStatus,
        LocationTrackingStatus expectedStatus,
        bool expectedPermissionDenied)
    {
        var result = LocationTrackingStatusMapper.FromPermissionStatus(permissionStatus);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedPermissionDenied, result.PermissionDenied);
    }

    [Fact]
    public void IsUsableLastKnownLocation_ReturnsFalseForStaleSimulatorLocation()
    {
        var staleLocation = new Location(37.3349, -122.0090)
        {
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10)
        };

        var isUsable = LocationTrackingStatusMapper.IsUsableLastKnownLocation(
            staleLocation,
            DateTimeOffset.UtcNow);

        Assert.False(isUsable);
    }

    [Fact]
    public void IsUsableLastKnownLocation_ReturnsTrueForFreshLocation()
    {
        var freshLocation = new Location(10.76042, 106.69321)
        {
            Timestamp = DateTimeOffset.UtcNow.AddSeconds(-30)
        };

        var isUsable = LocationTrackingStatusMapper.IsUsableLastKnownLocation(
            freshLocation,
            DateTimeOffset.UtcNow);

        Assert.True(isUsable);
    }
}
