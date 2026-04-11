namespace VinhKhanhGuide.Mobile.Models;

public class NearbyStallNotification
{
    public int StallId { get; init; }

    public string StallName { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public string DisplayImageSource => StallImageSourceResolver.Resolve(ImageUrl);

    public double DistanceMeters { get; init; }

    public ProximityTriggerReason TriggerReason { get; init; } = ProximityTriggerReason.EnteredRadius;

    public DateTimeOffset Timestamp { get; init; }

    public bool CanStartNarration { get; init; } = true;

    public bool AutoPlayStarted { get; init; }

    public string PromptText { get; init; } = string.Empty;

    public string NarrationText { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = "vi";

    public string PlaybackStatusText { get; init; } = string.Empty;

    public string DistanceText => $"{Math.Round(DistanceMeters)} m away";
}
