namespace VinhKhanhGuide.Mobile.Services;

public interface IAppUsageEnvironmentInfo
{
    string Platform { get; }

    string AppVersion { get; }
}
