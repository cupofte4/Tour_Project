using System.Threading.Channels;
using VinhKhanhGuide.Application.Analytics;

namespace VinhKhanhGuide.Infrastructure.Analytics;

public sealed class AudioPlayEventQueue : IAudioPlayEventQueue
{
    private readonly Channel<AudioPlayEvent> _channel = Channel.CreateUnbounded<AudioPlayEvent>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false
    });

    public ValueTask EnqueueAsync(AudioPlayEvent audioPlayEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audioPlayEvent);
        return _channel.Writer.WriteAsync(audioPlayEvent, cancellationToken);
    }

    public ValueTask<AudioPlayEvent> DequeueAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAsync(cancellationToken);

    public bool TryDequeue(out AudioPlayEvent? audioPlayEvent)
        => _channel.Reader.TryRead(out audioPlayEvent);
}
