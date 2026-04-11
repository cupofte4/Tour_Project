using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public delegate void NarrationPlaybackStateChangedEventHandler(NarrationPlaybackState state);

public interface INarrationService
{
    event NarrationPlaybackStateChangedEventHandler? PlaybackStateChanged;

    NarrationPlaybackState CurrentState { get; }

    bool IsPlaying { get; }

    Task RequestNarrationAsync(NarrationRequest request, CancellationToken cancellationToken = default);

    Task StopAsync();
}
