using Microsoft.Maui.ApplicationModel;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class NarrationService : INarrationService
{
    private readonly object _syncRoot = new();
    private readonly NarrationSessionManager _sessionManager;
    private readonly IDeviceTextToSpeech _deviceTextToSpeech;
    private readonly IAudioNarrationPlayer _audioNarrationPlayer;
    private CancellationTokenSource? _activeNarrationCts;
    private Task? _activeNarrationTask;

    public NarrationService(
        NarrationSessionManager sessionManager,
        IDeviceTextToSpeech deviceTextToSpeech,
        IAudioNarrationPlayer audioNarrationPlayer)
    {
        _sessionManager = sessionManager;
        _deviceTextToSpeech = deviceTextToSpeech;
        _audioNarrationPlayer = audioNarrationPlayer;
    }

    public event NarrationPlaybackStateChangedEventHandler? PlaybackStateChanged;

    public NarrationPlaybackState CurrentState => _sessionManager.CurrentState;

    public bool IsPlaying => CurrentState.Status is NarrationPlaybackStatus.Queued
        or NarrationPlaybackStatus.Preparing
        or NarrationPlaybackStatus.Playing;

    public async Task RequestNarrationAsync(NarrationRequest request, CancellationToken cancellationToken = default)
    {
        var transition = _sessionManager.BeginRequest(request);

        if (transition.Action == NarrationTransitionAction.IgnoreDuplicate)
        {
            return;
        }

        await PublishStateAsync(transition.State);

        if (transition.Action == NarrationTransitionAction.InterruptAndReplace)
        {
            await CancelActiveNarrationAsync();
        }

        var selection = _sessionManager.SelectContent(request, _audioNarrationPlayer);

        if (selection.ContentKind is null)
        {
            await PublishStateAsync(_sessionManager.MarkError(request.PoiId, "No narration content is available."));
            return;
        }

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task playbackTask;

        lock (_syncRoot)
        {
            _activeNarrationCts = linkedCts;
            playbackTask = _activeNarrationTask = RunNarrationAsync(request, selection, linkedCts.Token);
        }

        await playbackTask;
    }

    public async Task StopAsync()
    {
        var activePoiId = CurrentState.ActivePoiId;

        await CancelActiveNarrationAsync();
        await _audioNarrationPlayer.StopAsync();
        await PublishStateAsync(_sessionManager.MarkStopped(activePoiId, "Narration stopped."));
        await PublishStateAsync(_sessionManager.ResetToIdle());
    }

    private async Task RunNarrationAsync(
        NarrationRequest request,
        NarrationContentSelection selection,
        CancellationToken cancellationToken)
    {
        try
        {
            await PublishStateAsync(_sessionManager.MarkPreparing(request.PoiId, selection.ContentKind!.Value));
            await PublishStateAsync(_sessionManager.MarkPlaying(request.PoiId, selection.ContentKind.Value));

            if (selection.ContentKind == NarrationContentKind.Audio)
            {
                await _audioNarrationPlayer.PlayAsync(selection.Content, cancellationToken);
            }
            else
            {
                await _deviceTextToSpeech.SpeakAsync(selection.Content, request.LanguageCode, cancellationToken);
            }

            await PublishStateAsync(_sessionManager.MarkStopped(request.PoiId));
            await PublishStateAsync(_sessionManager.ResetToIdle());
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            await PublishStateAsync(_sessionManager.MarkError(request.PoiId, "Narration could not be played."));
        }
        finally
        {
            lock (_syncRoot)
            {
                _activeNarrationTask = null;
                _activeNarrationCts?.Dispose();
                _activeNarrationCts = null;
            }
        }
    }

    private async Task CancelActiveNarrationAsync()
    {
        CancellationTokenSource? activeNarrationCts;
        Task? activeNarrationTask;

        lock (_syncRoot)
        {
            activeNarrationCts = _activeNarrationCts;
            activeNarrationTask = _activeNarrationTask;
        }

        if (activeNarrationCts is null)
        {
            return;
        }

        activeNarrationCts.Cancel();

        if (activeNarrationTask is not null)
        {
            try
            {
                await activeNarrationTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task PublishStateAsync(NarrationPlaybackState state)
    {
        if (PlaybackStateChanged is null)
        {
            return;
        }

        if (MainThread.IsMainThread)
        {
            PlaybackStateChanged.Invoke(state);
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(() => PlaybackStateChanged.Invoke(state));
    }
}
