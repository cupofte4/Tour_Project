using Microsoft.EntityFrameworkCore;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Infrastructure.Persistence;

namespace VinhKhanhGuide.Infrastructure.Stalls;

public class StallReadRepository(AppDbContext dbContext) : IStallReadRepository
{
    public async Task<IReadOnlyList<StallDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Stalls
            .AsNoTracking()
            .OrderBy(stall => stall.Id)
            .Select(stall => new StallDto
            {
                Id = stall.Id,
                Name = stall.Name,
                DescriptionVi = stall.DescriptionVi,
                Latitude = stall.Latitude,
                Longitude = stall.Longitude,
                TriggerRadiusMeters = stall.TriggerRadiusMeters,
                Category = stall.Category,
                OpenHours = stall.OpenHours,
                ImageUrl = stall.ImageUrl,
                AverageRating = stall.AverageRating
            })
            .ToListAsync(cancellationToken);
    }

    public Task<StallDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Stalls
            .AsNoTracking()
            .Where(stall => stall.Id == id)
            .Select(stall => new StallDto
            {
                Id = stall.Id,
                Name = stall.Name,
                DescriptionVi = stall.DescriptionVi,
                Latitude = stall.Latitude,
                Longitude = stall.Longitude,
                TriggerRadiusMeters = stall.TriggerRadiusMeters,
                Category = stall.Category,
                OpenHours = stall.OpenHours,
                ImageUrl = stall.ImageUrl,
                AverageRating = stall.AverageRating
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
