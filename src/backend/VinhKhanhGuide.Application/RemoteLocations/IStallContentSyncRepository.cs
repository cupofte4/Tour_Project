using VinhKhanhGuide.Application.Stalls;

namespace VinhKhanhGuide.Application.RemoteLocations;

public interface IStallContentSyncRepository
{
    Task UpsertAsync(IReadOnlyCollection<StallDto> stalls, CancellationToken cancellationToken = default);
}
