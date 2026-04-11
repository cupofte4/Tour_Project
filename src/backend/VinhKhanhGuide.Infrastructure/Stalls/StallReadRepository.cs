using System.Text.Json;
using VinhKhanhGuide.Application.RemoteLocations;
using Microsoft.EntityFrameworkCore;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Domain.Entities;
using VinhKhanhGuide.Infrastructure.Persistence;

namespace VinhKhanhGuide.Infrastructure.Stalls;

public class StallReadRepository(AppDbContext dbContext) : IStallReadRepository, IStallContentSyncRepository
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
                Priority = stall.Priority,
                Category = stall.Category,
                OpenHours = stall.OpenHours,
                ImageUrl = stall.ImageUrl,
                ImageUrls = RemoteLocationToStallMapper.ParseJsonStringArray(stall.ImageUrlsJson),
                Address = stall.Address,
                Phone = stall.Phone,
                ReviewsJson = stall.ReviewsJson,
                MapLink = stall.MapLink,
                NarrationScriptVi = stall.NarrationScriptVi,
                AudioUrl = stall.AudioUrl,
                IsActive = stall.IsActive,
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
                Priority = stall.Priority,
                Category = stall.Category,
                OpenHours = stall.OpenHours,
                ImageUrl = stall.ImageUrl,
                ImageUrls = RemoteLocationToStallMapper.ParseJsonStringArray(stall.ImageUrlsJson),
                Address = stall.Address,
                Phone = stall.Phone,
                ReviewsJson = stall.ReviewsJson,
                MapLink = stall.MapLink,
                NarrationScriptVi = stall.NarrationScriptVi,
                AudioUrl = stall.AudioUrl,
                IsActive = stall.IsActive,
                AverageRating = stall.AverageRating
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task UpsertAsync(IReadOnlyCollection<StallDto> stalls, CancellationToken cancellationToken = default)
    {
        if (stalls.Count == 0)
        {
            return;
        }

        var stallIds = stalls.Select(stall => stall.Id).Distinct().ToArray();
        var existingStalls = await dbContext.Stalls
            .Where(stall => stallIds.Contains(stall.Id))
            .ToDictionaryAsync(stall => stall.Id, cancellationToken);
        var staleStalls = await dbContext.Stalls
            .Where(stall => !stallIds.Contains(stall.Id))
            .ToListAsync(cancellationToken);

        if (staleStalls.Count > 0)
        {
            var staleIds = staleStalls.Select(stall => stall.Id).ToArray();
            var staleTranslations = await dbContext.StallTranslations
                .Where(translation => staleIds.Contains(translation.StallId))
                .ToListAsync(cancellationToken);

            if (staleTranslations.Count > 0)
            {
                dbContext.StallTranslations.RemoveRange(staleTranslations);
            }

            dbContext.Stalls.RemoveRange(staleStalls);
        }

        foreach (var stall in stalls)
        {
            if (!existingStalls.TryGetValue(stall.Id, out var entity))
            {
                entity = new Stall
                {
                    Id = stall.Id
                };

                dbContext.Stalls.Add(entity);
                existingStalls[stall.Id] = entity;
            }

            entity.Name = stall.Name;
            entity.DescriptionVi = stall.DescriptionVi;
            entity.Latitude = stall.Latitude;
            entity.Longitude = stall.Longitude;
            entity.TriggerRadiusMeters = stall.TriggerRadiusMeters;
            entity.Priority = stall.Priority;
            entity.Category = stall.Category;
            entity.OpenHours = stall.OpenHours;
            entity.ImageUrl = stall.ImageUrl;
            entity.ImageUrlsJson = JsonSerializer.Serialize(stall.ImageUrls);
            entity.Address = stall.Address;
            entity.Phone = stall.Phone;
            entity.ReviewsJson = stall.ReviewsJson;
            entity.MapLink = stall.MapLink;
            entity.NarrationScriptVi = stall.NarrationScriptVi;
            entity.AudioUrl = stall.AudioUrl;
            entity.IsActive = stall.IsActive;
            entity.AverageRating = stall.AverageRating;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
