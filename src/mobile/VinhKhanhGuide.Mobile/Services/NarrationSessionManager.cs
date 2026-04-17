using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class NarrationSessionManager
{
    private readonly object _syncRoot = new();
    private int? _activePoiId;
    private int _activePriority;
    private string _activeRequestedLanguageCode = string.Empty;
    private string _activeLanguageCode = string.Empty;
    private string _activeLocaleCode = string.Empty;
    private string _activeText = string.Empty;
    private string _activeAudioUrl = string.Empty;
    private bool _activeUsedFallback;

    public NarrationPlaybackState CurrentState { get; private set; } = NarrationPlaybackState.Idle;

    public NarrationTransition BeginRequest(NarrationRequest request)
    {
        lock (_syncRoot)
        {
            if (_activePoiId == request.PoiId &&
                string.Equals(_activeRequestedLanguageCode, request.RequestedLanguageCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_activeLanguageCode, request.LanguageCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_activeLocaleCode, request.LocaleCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_activeText, request.Text, StringComparison.Ordinal) &&
                string.Equals(_activeAudioUrl, request.AudioUrl, StringComparison.Ordinal) &&
                _activeUsedFallback == request.UsedFallback &&
                (CurrentState.Status is NarrationPlaybackStatus.Queued
                    or NarrationPlaybackStatus.Preparing
                    or NarrationPlaybackStatus.Playing))
            {
                return new NarrationTransition(NarrationTransitionAction.IgnoreDuplicate, CurrentState);
            }

            var shouldInterrupt = _activePoiId.HasValue &&
                                  (CurrentState.Status is NarrationPlaybackStatus.Queued
                                      or NarrationPlaybackStatus.Preparing
                                      or NarrationPlaybackStatus.Playing);

            _activePoiId = request.PoiId;
            _activePriority = request.Priority;
            CaptureRequest(request);
            CurrentState = CreateState(
                NarrationPlaybackStatus.Queued,
                request.PoiId,
                null,
                shouldInterrupt
                    ? "Narration updated."
                    : string.Empty);

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
            CurrentState = CreateState(
                NarrationPlaybackStatus.Stopped,
                poiId,
                CurrentState.ContentKind,
                message);
            ClearActiveRequest();
            return CurrentState;
        }
    }

    public NarrationPlaybackState MarkError(int poiId, string message)
    {
        lock (_syncRoot)
        {
            CurrentState = CreateState(
                NarrationPlaybackStatus.Error,
                poiId,
                CurrentState.ContentKind,
                message);
            ClearActiveRequest();
            return CurrentState;
        }
    }

    public NarrationPlaybackState ResetToIdle()
    {
        lock (_syncRoot)
        {
            ClearActiveRequest();
            CurrentState = NarrationPlaybackState.Idle;
            return CurrentState;
        }
    }

    public NarrationContentSelection SelectContent(NarrationRequest request, IAudioNarrationPlayer audioNarrationPlayer)
    {
        if (TextToSpeechSettingsResolver.ShouldPreferTextToSpeech(request.LanguageCode) &&
            !string.IsNullOrWhiteSpace(request.Text))
        {
            return new NarrationContentSelection(NarrationContentKind.TextToSpeech, request.Text);
        }

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
                RequestedLanguageCode = _activeRequestedLanguageCode,
                LanguageCode = _activeLanguageCode,
                LocaleCode = _activeLocaleCode,
                UsedFallback = _activeUsedFallback,
                Message = message
            };

            return CurrentState;
        }
    }

    private void CaptureRequest(NarrationRequest request)
    {
        _activeRequestedLanguageCode = request.RequestedLanguageCode ?? string.Empty;
        _activeLanguageCode = request.LanguageCode ?? string.Empty;
        _activeLocaleCode = request.LocaleCode ?? string.Empty;
        _activeText = request.Text ?? string.Empty;
        _activeAudioUrl = request.AudioUrl ?? string.Empty;
        _activeUsedFallback = request.UsedFallback;
    }

    private void ClearActiveRequest()
    {
        _activePoiId = null;
        _activePriority = 0;
        _activeRequestedLanguageCode = string.Empty;
        _activeLanguageCode = string.Empty;
        _activeLocaleCode = string.Empty;
        _activeText = string.Empty;
        _activeAudioUrl = string.Empty;
        _activeUsedFallback = false;
    }

    private NarrationPlaybackState CreateState(
        NarrationPlaybackStatus status,
        int? poiId,
        NarrationContentKind? contentKind,
        string message)
    {
        return new NarrationPlaybackState
        {
            Status = status,
            ActivePoiId = poiId,
            ContentKind = contentKind,
            RequestedLanguageCode = _activeRequestedLanguageCode,
            LanguageCode = _activeLanguageCode,
            LocaleCode = _activeLocaleCode,
            UsedFallback = _activeUsedFallback,
            Message = message
        };
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
