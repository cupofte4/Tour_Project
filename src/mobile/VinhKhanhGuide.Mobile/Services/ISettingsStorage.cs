namespace VinhKhanhGuide.Mobile.Services;

public interface ISettingsStorage
{
    bool GetBool(string key, bool defaultValue);

    double GetDouble(string key, double defaultValue);

    string GetString(string key, string defaultValue);

    void SetBool(string key, bool value);

    void SetDouble(string key, double value);

    void SetString(string key, string value);
}
