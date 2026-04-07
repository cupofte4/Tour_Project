using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public class DeviceLocationService : ILocationService
{
    public async Task<LocationResult> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (permissionStatus != PermissionStatus.Granted)
        {
            return new LocationResult
            {
                PermissionDenied = true,
                Message = "Location permission was denied. Showing stall pins only."
            };
        }

        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();

            location ??= await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)),
                cancellationToken);

            if (location is null)
            {
                return new LocationResult
                {
                    Message = "Current location is unavailable. Showing stall pins only."
                };
            }

            return new LocationResult
            {
                CurrentLocation = new GeoPoint(location.Latitude, location.Longitude)
            };
        }
        catch (Exception)
        {
            return new LocationResult
            {
                Message = "Current location is unavailable. Showing stall pins only."
            };
        }
    }
}
