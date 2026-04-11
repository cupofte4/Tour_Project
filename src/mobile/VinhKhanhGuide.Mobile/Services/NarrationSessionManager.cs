using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class NarrationSessionManager
{
    private readonly object _syncRoot = new();
    private int? _activePoiId;
    private int _activePriority;

    public NarrationPlaybackState CurrentState { get; private set; } = NarrationPlaybackState.Idle;

    public NarrationTransition BeginRequest(NarrationRequest request)
    {
        lock (_syncRoot)
        {
            if (_activePoiId == request.PoiId &&
                CurrentState.Status is NarrationPlaybackStatus.Queued
                    or NarrationPlaybackStatus.Preparing
                    or NarrationPlaybackStatus.Playing)
            {
                return new NarrationTransition(NarrationTransitionAction.IgnoreDuplicate, CurrentState);
            }

            var shouldInterrupt = _activePoiId.HasValue &&
                                  _activePoiId != request.PoiId &&
                                  CurrentState.Status is NarrationPlaybackStatus.Queued
                                      or NarrationPlaybackStatus.Preparing
                                      or NarrationPlaybackStatus.Playing;

            _activePoiId = request.PoiId;
            _activePriority = request.Priority;
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Queued,
                ActivePoiId = request.PoiId,
                Message = shouldInterrupt
                    ? "Narration switched to another POI."
                    : string.Empty
            };

            return new NarrationTransition(
                shouldInterrupt ? NarrationTransitionAction.InterruptAndReplace : NarrationTransitionAction.StartNew,
                CurrentState);
        }
    }

    public NarrationPlaybackState MarkPreparing(int poiId, NarrationContentKind contentKind)
    {
        return UpdateActiveState(poiId, NarrationPlaybackStatus.Preparing, contentKind, string.Empty);
    }

    public NarrationPlaybackState MarkPlaying(int poiId, NarrationContentKind contentKind)
    {
        return UpdateActiveState(poiId, NarrationPlaybackStatus.Playing, contentKind, string.Empty);
    }

    public NarrationPlaybackState MarkStopped(int? poiId, string message = "")
    {
        lock (_syncRoot)
        {
            _activePoiId = null;
            _activePriority = 0;
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Stopped,
                ActivePoiId = poiId,
                Message = message
            };
            return CurrentState;
        }
    }

    public NarrationPlaybackState MarkError(int poiId, string message)
    {
        lock (_syncRoot)
        {
            _activePoiId = null;
            _activePriority = 0;
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Error,
                ActivePoiId = poiId,
                Message = message
            };
            return CurrentState;
        }
    }

    public NarrationPlaybackState ResetToIdle()
    {
        lock (_syncRoot)
        {
            _activePoiId = null;
            _activePriority = 0;
            CurrentState = NarrationPlaybackState.Idle;
            return CurrentState;
        }
    }

    public NarrationContentSelection SelectContent(NarrationRequest request, IAudioNarrationPlayer audioNarrationPlayer)
    {
        if (!string.IsNullOrWhiteSpace(request.AudioUrl) && audioNarrationPlayer.CanPlay(request.AudioUrl))
        {
            return new NarrationContentSelection(NarrationContentKind.Audio, request.AudioUrl);
        }

        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            return new NarrationContentSelection(NarrationContentKind.TextToSpeech, request.Text);
        }

        return new NarrationContentSelection(null, string.Empty);
    }

    private NarrationPlaybackState UpdateActiveState(
        int poiId,
        NarrationPlaybackStatus status,
        NarrationContentKind contentKind,
        string message)
    {
        lock (_syncRoot)
        {
            if (_activePoiId != poiId)
            {
                return CurrentState;
            }

            CurrentState = new NarrationPlaybackState
            {
                Status = status,
                ActivePoiId = poiId,
                ContentKind = contentKind,
                Message = message
            };

            return CurrentState;
        }
    }
}

public sealed record NarrationTransition(NarrationTransitionAction Action, NarrationPlaybackState State);

public enum NarrationTransitionAction
{
    IgnoreDuplicate = 0,
    StartNew = 1,
    InterruptAndReplace = 2
}

public sealed record NarrationContentSelection(NarrationContentKind? ContentKind, string Content);
