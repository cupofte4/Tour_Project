using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IProximityNarrationCoordinator
{
    NearbyStallNotification? CurrentPrompt { get; }

    Task<NearbyStallNotification?> HandleTriggerAsync(
        NearbyStallNotification notification,
        IEnumerable<StallSummary> stalls,
        CancellationToken cancellationToken = default);

    NearbyStallNotification? DismissPrompt();

    Task<NearbyStallNotification?> StartPromptNarrationAsync(
        IEnumerable<StallSummary> stalls,
        CancellationToken cancellationToken = default);
}
