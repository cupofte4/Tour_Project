namespace VinhKhanhGuide.Application.RemoteLocations;

public interface IRemoteLocationSyncService
{
    Task<RemoteLocationSyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
