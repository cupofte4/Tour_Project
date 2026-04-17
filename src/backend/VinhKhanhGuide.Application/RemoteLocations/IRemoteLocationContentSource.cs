namespace VinhKhanhGuide.Application.RemoteLocations;

public interface IRemoteLocationContentSource
{
    Task<IReadOnlyList<RemoteLocationRecord>> GetLocationsAsync(CancellationToken cancellationToken = default);
}
