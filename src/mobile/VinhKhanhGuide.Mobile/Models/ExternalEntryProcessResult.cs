namespace VinhKhanhGuide.Mobile.Models;

public sealed class ExternalEntryProcessResult
{
    public bool Succeeded { get; init; }

    public ExternalEntryOutcome Outcome { get; init; }

    public ExternalStallRoute Route { get; init; } = ExternalStallRoute.Invalid(
        string.Empty,
        ExternalEntrySourceType.Manual,
        "Invalid payload.");

    public string UserMessage { get; init; } = string.Empty;

    public bool IsDuplicate => Outcome == ExternalEntryOutcome.IgnoredDuplicate;
}
