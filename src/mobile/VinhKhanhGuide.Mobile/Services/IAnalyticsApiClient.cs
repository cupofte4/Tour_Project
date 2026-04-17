using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IAnalyticsApiClient
{
    Task PostEventAsync(AppUsageEventRequest request, CancellationToken cancellationToken = default);

    Task PostHeartbeatAsync(AppUsageHeartbeatRequest request, CancellationToken cancellationToken = default);
}
