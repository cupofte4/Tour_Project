namespace VinhKhanhGuide.Mobile.Models;

public class LocationResult
{
    public GeoPoint? CurrentLocation { get; init; }

    public string Message { get; init; } = string.Empty;

    public bool PermissionDenied { get; init; }
}
