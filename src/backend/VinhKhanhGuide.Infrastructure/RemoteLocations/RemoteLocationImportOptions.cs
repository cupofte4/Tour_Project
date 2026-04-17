namespace VinhKhanhGuide.Infrastructure.RemoteLocations;

public sealed class RemoteLocationImportOptions
{
    public bool Enabled { get; set; }

    public bool RunOnStartup { get; set; }

    public string SqlSnapshotPath { get; set; } = "foodguide.sql";
}
