using Microsoft.Maui.Storage;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class PreferencesSettingsStorage : ISettingsStorage
{
    public bool GetBool(string key, bool defaultValue)
    {
        return Preferences.Default.Get(key, defaultValue);
    }

    public double GetDouble(string key, double defaultValue)
    {
        return Preferences.Default.Get(key, defaultValue);
    }

    public string GetString(string key, string defaultValue)
    {
        return Preferences.Default.Get(key, defaultValue);
    }

    public void SetBool(string key, bool value)
    {
        Preferences.Default.Set(key, value);
    }

    public void SetDouble(string key, double value)
    {
        Preferences.Default.Set(key, value);
    }

    public void SetString(string key, string value)
    {
        Preferences.Default.Set(key, value);
    }
}
