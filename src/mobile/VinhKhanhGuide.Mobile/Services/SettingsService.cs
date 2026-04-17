using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class SettingsService : ISettingsService
{
    private const string GpsTrackingEnabledKey = "settings.gps.enabled";
    private const string TriggerRadiusMultiplierKey = "settings.proximity.radiusMultiplier";
    private const string AutoNarrationEnabledKey = "settings.narration.autoEnabled";
    private const string ApiBaseUrlOverrideKey = "settings.api.baseUrlOverride";
    private const string PreferredTtsLanguageKey = "settings.tts.language";
    private const string PreferredTtsVoiceIdKey = "settings.tts.voiceId";

    private readonly ISettingsStorage _storage;
    private readonly StallApiOptions _stallApiOptions;
    private AppSettings? _cachedSettings;

    public SettingsService(ISettingsStorage storage, Microsoft.Extensions.Options.IOptions<StallApiOptions> stallApiOptions)
    {
        _storage = storage;
        _stallApiOptions = stallApiOptions.Value;
    }

    public event AppSettingsChangedEventHandler? SettingsChanged;

    public AppSettings GetSettings()
    {
        _cachedSettings ??= Normalize(new AppSettings
        {
            IsGpsTrackingEnabled = _storage.GetBool(GpsTrackingEnabledKey, true),
            TriggerRadiusMultiplier = _storage.GetDouble(TriggerRadiusMultiplierKey, 1.0),
            AutoNarrationEnabled = _storage.GetBool(AutoNarrationEnabledKey, true),
            ApiBaseUrlOverride = _storage.GetString(ApiBaseUrlOverrideKey, string.Empty),
            PreferredTtsLanguage = _storage.GetString(PreferredTtsLanguageKey, string.Empty),
            PreferredTtsVoiceId = _storage.GetString(PreferredTtsVoiceIdKey, string.Empty)
        });

        return _cachedSettings;
    }

    public void SaveSettings(AppSettings settings)
    {
        var normalized = Normalize(settings);
        _storage.SetBool(GpsTrackingEnabledKey, normalized.IsGpsTrackingEnabled);
        _storage.SetDouble(TriggerRadiusMultiplierKey, normalized.TriggerRadiusMultiplier);
        _storage.SetBool(AutoNarrationEnabledKey, normalized.AutoNarrationEnabled);
        _storage.SetString(ApiBaseUrlOverrideKey, normalized.ApiBaseUrlOverride);
        _storage.SetString(PreferredTtsLanguageKey, normalized.PreferredTtsLanguage);
        _storage.SetString(PreferredTtsVoiceIdKey, normalized.PreferredTtsVoiceId);
        _cachedSettings = normalized;
        SettingsChanged?.Invoke(normalized);
    }

    public string GetResolvedApiBaseUrl()
    {
        var settings = GetSettings();

        if (StallApiOptions.TryNormalizeBaseUrl(settings.ApiBaseUrlOverride, out var overrideBaseUrl))
        {
            return overrideBaseUrl;
        }

        if (StallApiOptions.TryNormalizeBaseUrl(_stallApiOptions.BaseUrl, out var configuredBaseUrl))
        {
            return configuredBaseUrl;
        }

        throw new InvalidOperationException("Stall API base URL is not configured.");
    }

    public string GetConfiguredApiBaseUrl()
    {
        return StallApiOptions.TryNormalizeBaseUrl(_stallApiOptions.BaseUrl, out var configuredBaseUrl)
            ? configuredBaseUrl
            : string.Empty;
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
            ApiBaseUrlOverride = NormalizeApiBaseUrl(settings.ApiBaseUrlOverride),
            PreferredTtsLanguage = settings.PreferredTtsLanguage.Trim(),
            PreferredTtsVoiceId = settings.PreferredTtsVoiceId.Trim()
        };
    }

    private static string NormalizeApiBaseUrl(string apiBaseUrlOverride)
    {
        return StallApiOptions.TryNormalizeBaseUrl(apiBaseUrlOverride, out var normalizedBaseUrl)
            ? normalizedBaseUrl
            : string.Empty;
    }
}
