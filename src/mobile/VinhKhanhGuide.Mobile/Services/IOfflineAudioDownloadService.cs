using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IOfflineAudioDownloadService
{
    bool CanDownloadAudio(StallDetail? stallDetail);

    string GetDeterministicLocalPath(StallDetail stallDetail);

    Task<OfflineAudioDownloadResult> DownloadAsync(StallDetail stallDetail, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(string? localAudioPath, CancellationToken cancellationToken = default);
}
