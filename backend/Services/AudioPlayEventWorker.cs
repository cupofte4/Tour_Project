using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VinhKhanhGuide.Application.Analytics;

namespace VinhKhanhGuide.Infrastructure.Analytics;

public sealed class AudioPlayEventWorker : BackgroundService
{
    private const int MaxBatchSize = 50;

    private readonly IAudioPlayEventQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AudioPlayEventWorker> _logger;

    public AudioPlayEventWorker(
        IAudioPlayEventQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<AudioPlayEventWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audio play event worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var batch = new List<AudioPlayEvent>
                {
                    await _queue.DequeueAsync(stoppingToken)
                };

                while (batch.Count < MaxBatchSize && _queue.TryDequeue(out var nextEvent) && nextEvent is not null)
                {
                    batch.Add(nextEvent);
                }

                await ProcessBatchWithFallbackAsync(batch, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing audio play event queue.");
            }
        }

        _logger.LogInformation("Audio play event worker stopped.");
    }

    private async Task ProcessBatchWithFallbackAsync(IReadOnlyCollection<AudioPlayEvent> batch, CancellationToken cancellationToken)
    {
        try
        {
            await ProcessBatchAsync(batch, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process audio play batch of {BatchSize}. Falling back to single-event processing.", batch.Count);

            foreach (var audioPlayEvent in batch)
            {
                try
                {
                    await ProcessBatchAsync(new[] { audioPlayEvent }, cancellationToken);
                }
                catch (Exception itemEx)
                {
                    _logger.LogError(itemEx, "Failed to process queued audio play event for location {LocationId}.", audioPlayEvent.LocationId);
                }
            }
        }
    }

    private async Task ProcessBatchAsync(IReadOnlyCollection<AudioPlayEvent> batch, CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var analyticsService = scope.ServiceProvider.GetRequiredService<AnalyticsService>();

        var requests = batch.Select(item => new AudioPlayRequest
        {
            DeviceId = item.DeviceId,
            LocationId = item.LocationId,
            AudioId = item.AudioId,
            OccurredAtUtc = item.OccurredAtUtc
        }).ToArray();

        await analyticsService.RecordAudioPlayBatchAsync(requests, cancellationToken);
    }
}
