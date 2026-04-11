using Microsoft.EntityFrameworkCore;
using Npgsql;
using VinhKhanhGuide.Application.Translations;
using VinhKhanhGuide.Domain.Entities;
using VinhKhanhGuide.Infrastructure.Persistence;

namespace VinhKhanhGuide.Infrastructure.Translations;

public class StallTranslationRepository(AppDbContext dbContext) : IStallTranslationRepository
{
    public async Task<IReadOnlyList<StallTranslationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.StallTranslations
                .AsNoTracking()
                .Select(translation => new StallTranslationDto
                {
                    StallId = translation.StallId,
                    LanguageCode = translation.LanguageCode,
                    Name = translation.Name,
                    Description = translation.Description
                })
                .ToListAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return [];
        }
    }

    public async Task<StallTranslationDto?> GetByStallIdAndLanguageAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.StallTranslations
                .AsNoTracking()
                .Where(translation => translation.StallId == stallId && translation.LanguageCode == languageCode)
                .Select(translation => new StallTranslationDto
                {
                    StallId = translation.StallId,
                    LanguageCode = translation.LanguageCode,
                    Name = translation.Name,
                    Description = translation.Description
                })
                .SingleOrDefaultAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return null;
        }
    }

    public async Task SaveAsync(StallTranslationDto translation, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingTranslation = await dbContext.StallTranslations
                .SingleOrDefaultAsync(
                    item => item.StallId == translation.StallId && item.LanguageCode == translation.LanguageCode,
                    cancellationToken);

            if (existingTranslation is null)
            {
                dbContext.StallTranslations.Add(new StallTranslation
                {
                    StallId = translation.StallId,
                    LanguageCode = translation.LanguageCode,
                    Name = translation.Name,
                    Description = translation.Description,
                    LastGeneratedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                existingTranslation.Name = translation.Name;
                existingTranslation.Description = translation.Description;
                existingTranslation.LastGeneratedAt = DateTimeOffset.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            // Allow live translation responses even when the optional cache table
            // has not been created in the local database yet.
        }
    }

    public async Task DeleteAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingTranslation = await dbContext.StallTranslations
                .SingleOrDefaultAsync(
                    item => item.StallId == stallId && item.LanguageCode == languageCode,
                    cancellationToken);

            if (existingTranslation is null)
            {
                return;
            }

            dbContext.StallTranslations.Remove(existingTranslation);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UndefinedTable)
        {
        }
    }
}
