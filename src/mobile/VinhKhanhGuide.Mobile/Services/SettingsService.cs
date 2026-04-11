using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class SettingsService : ISettingsService
{
    private const string GpsTrackingEnabledKey = "settings.gps.enabled";
    private const string TriggerRadiusMultiplierKey = "settings.proximity.radiusMultiplier";
    private const string AutoNarrationEnabledKey = "settings.narration.autoEnabled";
    private const string PreferredTtsLanguageKey = "settings.tts.language";
    private const string PreferredTtsVoiceIdKey = "settings.tts.voiceId";

    private readonly ISettingsStorage _storage;

    public SettingsService(ISettingsStorage storage)
    {
        _storage = storage;
    }

    public AppSettings GetSettings()
    {
        return Normalize(new AppSettings
        {
            IsGpsTrackingEnabled = _storage.GetBool(GpsTrackingEnabledKey, true),
            TriggerRadiusMultiplier = _storage.GetDouble(TriggerRadiusMultiplierKey, 1.0),
            AutoNarrationEnabled = _storage.GetBool(AutoNarrationEnabledKey, true),
            PreferredTtsLanguage = _storage.GetString(PreferredTtsLanguageKey, string.Empty),
            PreferredTtsVoiceId = _storage.GetString(PreferredTtsVoiceIdKey, string.Empty)
        });
    }

    public void SaveSettings(AppSettings settings)
    {
        var normalized = Normalize(settings);
        _storage.SetBool(GpsTrackingEnabledKey, normalized.IsGpsTrackingEnabled);
        _storage.SetDouble(TriggerRadiusMultiplierKey, normalized.TriggerRadiusMultiplier);
        _storage.SetBool(AutoNarrationEnabledKey, normalized.AutoNarrationEnabled);
        _storage.SetString(PreferredTtsLanguageKey, normalized.PreferredTtsLanguage);
        _storage.SetString(PreferredTtsVoiceIdKey, normalized.PreferredTtsVoiceId);
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        var triggerRadiusMultiplier = settings.TriggerRadiusMultiplier;

        if (triggerRadiusMultiplier < 0.5d)
        {
            triggerRadiusMultiplier = 0.5d;
        }
        else if (triggerRadiusMultiplier > 2.0d)
        {
            triggerRadiusMultiplier = 2.0d;
        }

        return new AppSettings
        {
            IsGpsTrackingEnabled = settings.IsGpsTrackingEnabled,
            TriggerRadiusMultiplier = Math.Round(triggerRadiusMultiplier, 2, MidpointRounding.AwayFromZero),
            AutoNarrationEnabled = settings.AutoNarrationEnabled,
            PreferredTtsLanguage = settings.PreferredTtsLanguage.Trim(),
            PreferredTtsVoiceId = settings.PreferredTtsVoiceId.Trim()
        };
    }
}
