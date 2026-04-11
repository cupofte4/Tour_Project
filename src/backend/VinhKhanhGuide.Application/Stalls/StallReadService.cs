namespace VinhKhanhGuide.Application.Stalls;

public class StallReadService(
    IStallReadRepository stallReadRepository,
    IStallDistanceCalculator stallDistanceCalculator) : IStallReadService
{
    public Task<IReadOnlyList<StallDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return stallReadRepository.GetAllAsync(cancellationToken);
    }

    public Task<StallDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return stallReadRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<NearbyStallDto>> GetNearbyAsync(NearbyStallQueryDto query, CancellationToken cancellationToken = default)
    {
        var stalls = await stallReadRepository.GetAllAsync(cancellationToken);

        return stalls
            .Select(stall => new NearbyStallDto
            {
                Id = stall.Id,
                Name = stall.Name,
                DescriptionVi = stall.DescriptionVi,
                Latitude = stall.Latitude,
                Longitude = stall.Longitude,
                TriggerRadiusMeters = stall.TriggerRadiusMeters,
                Priority = stall.Priority,
                Category = stall.Category,
                OpenHours = stall.OpenHours,
                ImageUrl = stall.ImageUrl,
                ImageUrls = stall.ImageUrls,
                Address = stall.Address,
                Phone = stall.Phone,
                ReviewsJson = stall.ReviewsJson,
                MapLink = stall.MapLink,
                NarrationScriptVi = stall.NarrationScriptVi,
                AudioUrl = stall.AudioUrl,
                IsActive = stall.IsActive,
                AverageRating = stall.AverageRating,
                DistanceMeters = stallDistanceCalculator.CalculateMeters(
                    query.Latitude,
                    query.Longitude,
                    stall.Latitude,
                    stall.Longitude)
            })
            .Where(stall => stall.DistanceMeters <= query.RadiusMeters)
            .OrderBy(stall => stall.DistanceMeters)
            .ToList();
    }
}
