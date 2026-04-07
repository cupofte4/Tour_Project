namespace VinhKhanhGuide.Mobile.Models;

public class NearbyStallNotification
{
    public int StallId { get; init; }

    public string StallName { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public double DistanceMeters { get; init; }

    public string DistanceText => $"{Math.Round(DistanceMeters)} m away";
}
