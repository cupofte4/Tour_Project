using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface ILocationService
{
    Task<LocationResult> GetCurrentLocationAsync(CancellationToken cancellationToken = default);
}
