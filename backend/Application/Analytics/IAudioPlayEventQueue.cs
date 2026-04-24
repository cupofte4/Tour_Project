namespace VinhKhanhGuide.Application.Analytics;

public interface IAudioPlayEventQueue
{
    ValueTask EnqueueAsync(AudioPlayEvent audioPlayEvent, CancellationToken cancellationToken = default);
    ValueTask<AudioPlayEvent> DequeueAsync(CancellationToken cancellationToken);
    bool TryDequeue(out AudioPlayEvent? audioPlayEvent);
}
