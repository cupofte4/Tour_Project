using Microsoft.Maui.ApplicationModel;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class DeviceAppUsageEnvironmentInfo : IAppUsageEnvironmentInfo
{
    public string Platform => DeviceInfo.Current.Platform.ToString().ToLowerInvariant();

    public string AppVersion => AppInfo.Current.VersionString;
}
