namespace VinhKhanhGuide.Application.RemoteLocations;

public sealed class RemoteLocationSyncResult
{
    // "Skipped" is reported as unchanged to make repeated imports explicitly idempotent.
    public int SourceRecordCount { get; init; }

    public int DistinctSourceRecordCount { get; init; }

    public int InsertedStallCount { get; init; }

    public int UpdatedStallCount { get; init; }

    public int DeactivatedStallCount { get; init; }

    public int UnchangedStallCount { get; init; }

    public int SkippedStallCount { get; init; }

    public int InsertedTranslationCount { get; init; }

    public int UpdatedTranslationCount { get; init; }

    public int RemovedTranslationCount { get; init; }

    public int UnchangedTranslationCount { get; init; }

    public int SkippedTranslationCount { get; init; }

    public int FailedRecordCount { get; init; }

    public IReadOnlyList<int> SyncedIds { get; init; } = [];
}
