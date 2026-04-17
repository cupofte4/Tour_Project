namespace VinhKhanhGuide.Mobile.Models;

public enum LocationTrackingStatus
{
    Unknown = 0,
    Ready = 1,
    PermissionDenied = 2,
    PermissionRestricted = 3,
    LocationServicesDisabled = 4,
    TemporarilyUnavailable = 5,
    Unsupported = 6
}
