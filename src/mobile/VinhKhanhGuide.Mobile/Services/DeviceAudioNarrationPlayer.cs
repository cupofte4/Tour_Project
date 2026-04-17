using System.Threading.Tasks;
#if IOS
using AVFoundation;
using Foundation;
#endif

namespace VinhKhanhGuide.Mobile.Services;

public sealed class DeviceAudioNarrationPlayer : IAudioNarrationPlayer
{
#if IOS
    private AVPlayer? _audioPlayer;
#endif

    public bool CanPlay(string audioUrl)
    {
        if (string.IsNullOrWhiteSpace(audioUrl))
        {
            return false;
        }

        if (File.Exists(audioUrl))
        {
            return true;
        }

        return Uri.TryCreate(audioUrl, UriKind.Absolute, out var audioUri) &&
               (string.Equals(audioUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(audioUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
    }

    public Task PlayAsync(string audioUrl, CancellationToken cancellationToken)
    {
        if (!CanPlay(audioUrl))
        {
            return Task.CompletedTask;
        }

#if IOS
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var url = File.Exists(audioUrl)
                ? NSUrl.FromFilename(audioUrl)
                : NSUrl.FromString(audioUrl);

            if (url is null)
            {
                return;
            }

            _audioPlayer?.Pause();
            _audioPlayer?.Dispose();
            _audioPlayer = new AVPlayer(url);
            _audioPlayer?.Play();
        });
#else
        return Task.CompletedTask;
#endif
    }

    public Task StopAsync()
    {
#if IOS
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            _audioPlayer?.Pause();
            _audioPlayer?.Dispose();
            _audioPlayer = null;
        });
#else
        return Task.CompletedTask;
#endif
    }
}
