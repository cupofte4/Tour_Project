namespace VinhKhanhGuide.Application.RemoteLocations;

public sealed class RemoteLocationSyncResult
{
    public int SourceRecordCount { get; init; }

    public int ImportedStallCount { get; init; }

    public int ImportedTranslationCount { get; init; }

    public IReadOnlyList<int> SyncedIds { get; init; } = [];
}
