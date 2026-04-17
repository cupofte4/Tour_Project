namespace VinhKhanhGuide.Mobile.Services;

public interface IAudioNarrationPlayer
{
    bool CanPlay(string audioUrl);

    Task PlayAsync(string audioUrl, CancellationToken cancellationToken);

    Task StopAsync();
}
