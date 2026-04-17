namespace VinhKhanhGuide.Mobile.Services;

public sealed record OfflineAudioDownloadResult(
    bool Succeeded,
    string LocalAudioPath,
    string Message)
{
    public static OfflineAudioDownloadResult Success(string localAudioPath)
    {
        return new OfflineAudioDownloadResult(true, localAudioPath, string.Empty);
    }

    public static OfflineAudioDownloadResult Failure(string message)
    {
        return new OfflineAudioDownloadResult(false, string.Empty, message);
    }
}
