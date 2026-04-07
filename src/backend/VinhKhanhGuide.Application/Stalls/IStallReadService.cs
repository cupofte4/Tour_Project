namespace VinhKhanhGuide.Application.Stalls;

public interface IStallReadService
{
    Task<IReadOnlyList<StallDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<StallDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NearbyStallDto>> GetNearbyAsync(NearbyStallQueryDto query, CancellationToken cancellationToken = default);
}
