namespace VinhKhanhGuide.Mobile.Models;

public sealed class AppSettings
{
    public bool IsGpsTrackingEnabled { get; init; } = true;

    public double TriggerRadiusMultiplier { get; init; } = 1.0;

    public bool AutoNarrationEnabled { get; init; } = true;

    public string ApiBaseUrlOverride { get; init; } = string.Empty;

    public string PreferredTtsLanguage { get; init; } = string.Empty;

    public string PreferredTtsVoiceId { get; init; } = string.Empty;
}
