using Microsoft.Maui.Devices.Sensors;

namespace VinhKhanhGuide.Mobile.Models;

public class LocationTrackingOptions
{
    public GeolocationAccuracy Accuracy { get; set; } = GeolocationAccuracy.Medium;

    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public bool UseLastKnownLocationFirst { get; set; } = true;

    public bool EnableBackgroundUpdates { get; set; }
}
