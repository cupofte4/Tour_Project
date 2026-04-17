using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class NoopOfflineAudioDownloadService : IOfflineAudioDownloadService
{
    public bool CanDownloadAudio(StallDetail? stallDetail)
    {
        return stallDetail is not null && Uri.TryCreate(stallDetail.AudioUrl, UriKind.Absolute, out _);
    }

    public string GetDeterministicLocalPath(StallDetail stallDetail)
    {
        return string.Empty;
    }

    public Task<OfflineAudioDownloadResult> DownloadAsync(StallDetail stallDetail, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OfflineAudioDownloadResult.Failure("Offline audio is not available for this location."));
    }

    public Task<bool> RemoveAsync(string? localAudioPath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
