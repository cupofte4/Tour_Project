using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void GetSettings_ReturnsDefaults_WhenNothingPersisted()
    {
        var service = new SettingsService(new FakeSettingsStorage());

        var settings = service.GetSettings();

        Assert.True(settings.IsGpsTrackingEnabled);
        Assert.Equal(1.0d, settings.TriggerRadiusMultiplier);
        Assert.True(settings.AutoNarrationEnabled);
        Assert.Equal(string.Empty, settings.PreferredTtsLanguage);
        Assert.Equal(string.Empty, settings.PreferredTtsVoiceId);
    }

    [Fact]
    public void SaveSettings_RoundTripsPersistedValues()
    {
        var storage = new FakeSettingsStorage();
        var service = new SettingsService(storage);

        service.SaveSettings(new AppSettings
        {
            IsGpsTrackingEnabled = false,
            TriggerRadiusMultiplier = 1.6d,
            AutoNarrationEnabled = false,
            PreferredTtsLanguage = "en-US",
            PreferredTtsVoiceId = "voice-1"
        });

        var settings = service.GetSettings();

        Assert.False(settings.IsGpsTrackingEnabled);
        Assert.Equal(1.6d, settings.TriggerRadiusMultiplier);
        Assert.False(settings.AutoNarrationEnabled);
        Assert.Equal("en-US", settings.PreferredTtsLanguage);
        Assert.Equal("voice-1", settings.PreferredTtsVoiceId);
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
