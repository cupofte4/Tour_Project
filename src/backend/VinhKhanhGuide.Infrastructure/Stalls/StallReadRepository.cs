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

    public async Task<StallSyncPersistenceResult> UpsertAsync(IReadOnlyCollection<StallDto> stalls, CancellationToken cancellationToken = default)
    {
        if (stalls.Count == 0)
        {
            return new StallSyncPersistenceResult();
        }

        var stallIds = stalls.Select(stall => stall.Id).Distinct().ToArray();
        var existingStalls = await dbContext.Stalls
            .ToDictionaryAsync(stall => stall.Id, cancellationToken);
        var insertedCount = 0;
        var updatedCount = 0;
        var deactivatedCount = 0;
        var unchangedCount = 0;

        foreach (var staleStall in existingStalls.Values.Where(stall => !stallIds.Contains(stall.Id) && stall.IsActive))
        {
            staleStall.IsActive = false;
            deactivatedCount++;
        }

        foreach (var stall in stalls)
        {
            if (!existingStalls.TryGetValue(stall.Id, out var entity))
            {
                entity = new Stall
                {
                    Id = stall.Id
                };

                ApplyValues(entity, stall);
                dbContext.Stalls.Add(entity);
                existingStalls[stall.Id] = entity;
                insertedCount++;
                continue;
            }

            var imageUrlsJson = JsonSerializer.Serialize(stall.ImageUrls);
            var changed = entity.Name != stall.Name ||
                          entity.DescriptionVi != stall.DescriptionVi ||
                          entity.Latitude != stall.Latitude ||
                          entity.Longitude != stall.Longitude ||
                          entity.TriggerRadiusMeters != stall.TriggerRadiusMeters ||
                          entity.Priority != stall.Priority ||
                          entity.Category != stall.Category ||
                          entity.OpenHours != stall.OpenHours ||
                          entity.ImageUrl != stall.ImageUrl ||
                          entity.ImageUrlsJson != imageUrlsJson ||
                          entity.Address != stall.Address ||
                          entity.Phone != stall.Phone ||
                          entity.ReviewsJson != stall.ReviewsJson ||
                          entity.MapLink != stall.MapLink ||
                          entity.NarrationScriptVi != stall.NarrationScriptVi ||
                          entity.AudioUrl != stall.AudioUrl ||
                          entity.IsActive != true ||
                          entity.AverageRating != stall.AverageRating;

            if (!changed)
            {
                unchangedCount++;
                continue;
            }

            ApplyValues(entity, stall);
            updatedCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new StallSyncPersistenceResult
        {
            InsertedCount = insertedCount,
            UpdatedCount = updatedCount,
            DeactivatedCount = deactivatedCount,
            UnchangedCount = unchangedCount
        };
    }

    private static void ApplyValues(Stall entity, StallDto stall)
    {
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
        entity.IsActive = true;
        entity.AverageRating = stall.AverageRating;
    }
}
