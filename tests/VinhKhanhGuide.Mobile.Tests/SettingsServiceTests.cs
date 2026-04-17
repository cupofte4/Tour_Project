using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;
using Microsoft.Extensions.Options;

namespace VinhKhanhGuide.Mobile.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void GetSettings_ReturnsDefaults_WhenNothingPersisted()
    {
        var service = CreateService(new FakeSettingsStorage());

        var settings = service.GetSettings();

        Assert.True(settings.IsGpsTrackingEnabled);
        Assert.Equal(1.0d, settings.TriggerRadiusMultiplier);
        Assert.True(settings.AutoNarrationEnabled);
        Assert.Equal(string.Empty, settings.ApiBaseUrlOverride);
        Assert.Equal(string.Empty, settings.PreferredTtsLanguage);
        Assert.Equal(string.Empty, settings.PreferredTtsVoiceId);
    }

    [Fact]
    public void SaveSettings_RoundTripsPersistedValues()
    {
        var storage = new FakeSettingsStorage();
        var service = CreateService(storage);

        service.SaveSettings(new AppSettings
        {
            IsGpsTrackingEnabled = false,
            TriggerRadiusMultiplier = 1.6d,
            AutoNarrationEnabled = false,
            ApiBaseUrlOverride = " https://demo.example/api ",
            PreferredTtsLanguage = "en-US",
            PreferredTtsVoiceId = "voice-1"
        });

        var settings = service.GetSettings();

        Assert.False(settings.IsGpsTrackingEnabled);
        Assert.Equal(1.6d, settings.TriggerRadiusMultiplier);
        Assert.False(settings.AutoNarrationEnabled);
        Assert.Equal("https://demo.example/api/", settings.ApiBaseUrlOverride);
        Assert.Equal("en-US", settings.PreferredTtsLanguage);
        Assert.Equal("voice-1", settings.PreferredTtsVoiceId);
    }

    [Fact]
    public void GetResolvedApiBaseUrl_UsesPersistedOverride_ThenFallsBackToConfiguredDefault()
    {
        var storage = new FakeSettingsStorage();
        var service = CreateService(storage, "https://demo.example/");

        Assert.Equal("https://demo.example/", service.GetResolvedApiBaseUrl());

        service.SaveSettings(new AppSettings
        {
            ApiBaseUrlOverride = "https://demo.example/api"
        });

        Assert.Equal("https://demo.example/api/", service.GetResolvedApiBaseUrl());
    }

    [Fact]
    public void GetConfiguredApiBaseUrl_ReturnsNormalizedConfiguredValue()
    {
        var service = CreateService(new FakeSettingsStorage(), "https://public-tunnel.example/demo");

        Assert.Equal("https://public-tunnel.example/demo/", service.GetConfiguredApiBaseUrl());
    }

    private static SettingsService CreateService(FakeSettingsStorage storage, string defaultBaseUrl = "https://demo.example/")
    {
        return new SettingsService(
            storage,
            Options.Create(new StallApiOptions
            {
                BaseUrl = defaultBaseUrl
            }));
    }

    private sealed class FakeSettingsStorage : ISettingsStorage
    {
        private readonly Dictionary<string, object> _values = [];

        public bool GetBool(string key, bool defaultValue)
        {
            return _values.TryGetValue(key, out var value) ? (bool)value : defaultValue;
        }

        public double GetDouble(string key, double defaultValue)
        {
            return _values.TryGetValue(key, out var value) ? (double)value : defaultValue;
        }

        public string GetString(string key, string defaultValue)
        {
            return _values.TryGetValue(key, out var value) ? (string)value : defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            _values[key] = value;
        }

        public void SetDouble(string key, double value)
        {
            _values[key] = value;
        }

        public void SetString(string key, string value)
        {
            _values[key] = value;
        }
    }
}
