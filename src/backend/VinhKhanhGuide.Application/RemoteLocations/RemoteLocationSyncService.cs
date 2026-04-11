using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Application.RemoteLocations;

public sealed class RemoteLocationSyncService(
    IRemoteLocationContentSource remoteLocationContentSource,
    IStallContentSyncRepository stallContentSyncRepository,
    IStallTranslationRepository stallTranslationRepository) : IRemoteLocationSyncService
{
    public async Task<RemoteLocationSyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        var sourceRecords = await remoteLocationContentSource.GetLocationsAsync(cancellationToken);

        var mappedRecords = sourceRecords
            .GroupBy(location => location.Id)
            .Select(group => RemoteLocationToStallMapper.Map(group.Last()))
            .ToArray();

        await stallContentSyncRepository.UpsertAsync(
            mappedRecords.Select(record => record.Stall).ToArray(),
            cancellationToken);

        var sourceIds = mappedRecords.Select(record => record.Stall.Id).ToHashSet();
        var translationCount = 0;

        var existingTranslations = await stallTranslationRepository.GetAllAsync(cancellationToken);

        foreach (var translation in existingTranslations.Where(translation => !sourceIds.Contains(translation.StallId)))
        {
            await stallTranslationRepository.DeleteAsync(translation.StallId, translation.LanguageCode, cancellationToken);
        }

        foreach (var translation in mappedRecords.SelectMany(record => record.Translations))
        {
            await stallTranslationRepository.SaveAsync(translation, cancellationToken);
            translationCount++;
        }

        return new RemoteLocationSyncResult
        {
            SourceRecordCount = sourceRecords.Count,
            ImportedStallCount = mappedRecords.Length,
            ImportedTranslationCount = translationCount,
            SyncedIds = mappedRecords.Select(record => record.Stall.Id).OrderBy(id => id).ToArray()
        };
    }
}
