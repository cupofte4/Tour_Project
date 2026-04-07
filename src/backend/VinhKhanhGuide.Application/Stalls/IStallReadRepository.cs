namespace VinhKhanhGuide.Application.Stalls;

public interface IStallReadRepository
{
    Task<IReadOnlyList<StallDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<StallDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
