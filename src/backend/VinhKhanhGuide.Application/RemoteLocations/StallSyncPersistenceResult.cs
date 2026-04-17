namespace VinhKhanhGuide.Application.RemoteLocations;

public sealed class StallSyncPersistenceResult
{
    public int InsertedCount { get; init; }

    public int UpdatedCount { get; init; }

    public int DeactivatedCount { get; init; }

    public int UnchangedCount { get; init; }
}
