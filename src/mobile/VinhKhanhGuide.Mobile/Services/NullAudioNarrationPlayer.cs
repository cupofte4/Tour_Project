namespace VinhKhanhGuide.Mobile.Services;

public sealed class NullAudioNarrationPlayer : IAudioNarrationPlayer
{
    public bool CanPlay(string audioUrl)
    {
        return false;
    }

    public Task PlayAsync(string audioUrl, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}
