using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Application.RemoteLocations;

public sealed class RemoteLocationSyncService(
    IRemoteLocationContentSource remoteLocationContentSource,
    IStallContentSyncRepository stallContentSyncRepository,
    IStallTranslationRepository stallTranslationRepository) : IRemoteLocationSyncService
{
    private static readonly string[] SupportedTranslationLanguages = ["en", "zh", "de"];

    public async Task<RemoteLocationSyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        // This sync is intentionally idempotent: rerunning the same snapshot only skips unchanged rows,
        // while changed rows are updated and missing rows are deactivated/removed by existing repository behavior.
        var sourceRecords = await remoteLocationContentSource.GetLocationsAsync(cancellationToken);

        var mappedRecords = sourceRecords
            .GroupBy(location => location.Id)
            .Select(group => RemoteLocationToStallMapper.Map(group.Last()))
            .ToArray();

        var stallResult = await stallContentSyncRepository.UpsertAsync(
            mappedRecords.Select(record => record.Stall).ToArray(),
            cancellationToken);

        var existingTranslations = await stallTranslationRepository.GetAllAsync(cancellationToken);
        var desiredTranslations = mappedRecords
            .SelectMany(record => record.Translations)
            .ToDictionary(
                translation => CreateTranslationKey(translation.StallId, translation.LanguageCode),
                translation => translation,
                StringComparer.OrdinalIgnoreCase);
        var existingTranslationMap = existingTranslations
            .ToDictionary(
                translation => CreateTranslationKey(translation.StallId, translation.LanguageCode),
                translation => translation,
                StringComparer.OrdinalIgnoreCase);
        var removedTranslationCount = 0;
        var insertedTranslationCount = 0;
        var updatedTranslationCount = 0;
        var unchangedTranslationCount = 0;

        foreach (var translation in existingTranslations)
        {
            var key = CreateTranslationKey(translation.StallId, translation.LanguageCode);

            if (desiredTranslations.ContainsKey(key) ||
                !SupportedTranslationLanguages.Contains(translation.LanguageCode, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            await stallTranslationRepository.DeleteAsync(translation.StallId, translation.LanguageCode, cancellationToken);
            removedTranslationCount++;
        }

        foreach (var translation in desiredTranslations.Values)
        {
            var key = CreateTranslationKey(translation.StallId, translation.LanguageCode);

            if (!existingTranslationMap.TryGetValue(key, out var existingTranslation))
            {
                await stallTranslationRepository.SaveAsync(translation, cancellationToken);
                insertedTranslationCount++;
                continue;
            }

            if (string.Equals(existingTranslation.Name, translation.Name, StringComparison.Ordinal) &&
                string.Equals(existingTranslation.Description, translation.Description, StringComparison.Ordinal))
            {
                unchangedTranslationCount++;
                continue;
            }

            await stallTranslationRepository.SaveAsync(translation, cancellationToken);
            updatedTranslationCount++;
        }

        return new RemoteLocationSyncResult
        {
            SourceRecordCount = sourceRecords.Count,
            DistinctSourceRecordCount = mappedRecords.Length,
            InsertedStallCount = stallResult.InsertedCount,
            UpdatedStallCount = stallResult.UpdatedCount,
            DeactivatedStallCount = stallResult.DeactivatedCount,
            UnchangedStallCount = stallResult.UnchangedCount,
            SkippedStallCount = stallResult.UnchangedCount,
            InsertedTranslationCount = insertedTranslationCount,
            UpdatedTranslationCount = updatedTranslationCount,
            RemovedTranslationCount = removedTranslationCount,
            UnchangedTranslationCount = unchangedTranslationCount,
            SkippedTranslationCount = unchangedTranslationCount,
            FailedRecordCount = 0,
            SyncedIds = mappedRecords.Select(record => record.Stall.Id).OrderBy(id => id).ToArray()
        };
    }

    private static string CreateTranslationKey(int stallId, string languageCode)
    {
        return $"{stallId}:{languageCode.Trim().ToLowerInvariant()}";
    }
}
