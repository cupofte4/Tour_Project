namespace VinhKhanhGuide.Mobile.Models;

public class LocationResult
{
    public GeoPoint? CurrentLocation { get; init; }

    public string Message { get; init; } = string.Empty;

    public bool PermissionDenied { get; init; }

    public LocationTrackingStatus Status { get; init; } = LocationTrackingStatus.Unknown;

    public bool IsSuccessful => CurrentLocation is not null && Status == LocationTrackingStatus.Ready;
}
