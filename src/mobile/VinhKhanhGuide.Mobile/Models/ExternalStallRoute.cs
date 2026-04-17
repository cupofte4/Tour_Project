namespace VinhKhanhGuide.Mobile.Models;

public sealed class ExternalStallRoute
{
    public string RawPayload { get; init; } = string.Empty;

    public int StallId { get; init; }

    public bool AutoPlay { get; init; }

    public string? RequestedLanguageCode { get; init; }

    public ExternalEntrySourceType SourceType { get; init; }

    public bool IsValid { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public static ExternalStallRoute Invalid(
        string rawPayload,
        ExternalEntrySourceType sourceType,
        string errorMessage)
    {
        return new ExternalStallRoute
        {
            RawPayload = rawPayload,
            SourceType = sourceType,
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}
