using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class NarrationSessionManagerTests
{
    private readonly NarrationSessionManager _manager = new();

    [Fact]
    public void BeginRequest_DuplicateForActivePoi_IsIgnored()
    {
        var request = CreateRequest(1, 1, "", "Hello");

        var first = _manager.BeginRequest(request);
        _manager.MarkPreparing(1, NarrationContentKind.TextToSpeech);
        _manager.MarkPlaying(1, NarrationContentKind.TextToSpeech);
        var duplicate = _manager.BeginRequest(request);

        Assert.Equal(NarrationTransitionAction.StartNew, first.Action);
        Assert.Equal(NarrationTransitionAction.IgnoreDuplicate, duplicate.Action);
    }

    [Fact]
    public void BeginRequest_NewPoi_ReplacesCurrentSession()
    {
        var first = CreateRequest(1, 1, "", "First");
        var second = CreateRequest(2, 1, "", "Second");

        _manager.BeginRequest(first);
        _manager.MarkPlaying(1, NarrationContentKind.TextToSpeech);
        var replacement = _manager.BeginRequest(second);

        Assert.Equal(NarrationTransitionAction.InterruptAndReplace, replacement.Action);
        Assert.Equal(2, replacement.State.ActivePoiId);
        Assert.Equal(NarrationPlaybackStatus.Queued, replacement.State.Status);
    }

    [Fact]
    public void StateTransitions_AreCorrect_ForNormalFlow()
    {
        var request = CreateRequest(3, 1, "", "Narrate");

        var queued = _manager.BeginRequest(request).State;
        var preparing = _manager.MarkPreparing(3, NarrationContentKind.TextToSpeech);
        var playing = _manager.MarkPlaying(3, NarrationContentKind.TextToSpeech);
        var stopped = _manager.MarkStopped(3);
        var idle = _manager.ResetToIdle();

        Assert.Equal(NarrationPlaybackStatus.Queued, queued.Status);
        Assert.Equal(NarrationPlaybackStatus.Preparing, preparing.Status);
        Assert.Equal(NarrationPlaybackStatus.Playing, playing.Status);
        Assert.Equal(NarrationPlaybackStatus.Stopped, stopped.Status);
        Assert.Equal(NarrationPlaybackStatus.Idle, idle.Status);
    }

    [Fact]
    public void SelectContent_PrefersAudio_FirstThenTts()
    {
        var audioPlayer = new FakeAudioNarrationPlayer(canPlay: true);
        var audioRequest = CreateRequest(4, 1, "https://example.com/audio.mp3", "Fallback");
        var ttsRequest = CreateRequest(4, 1, "", "Fallback");

        var audioSelection = _manager.SelectContent(audioRequest, audioPlayer);
        var ttsSelection = _manager.SelectContent(ttsRequest, new FakeAudioNarrationPlayer(canPlay: false));

        Assert.Equal(NarrationContentKind.Audio, audioSelection.ContentKind);
        Assert.Equal(NarrationContentKind.TextToSpeech, ttsSelection.ContentKind);
    }

    [Fact]
    public void ResetToIdle_ClearsPlaybackState()
    {
        var request = CreateRequest(5, 1, "", "Narrate");

        _manager.BeginRequest(request);
        _manager.MarkPlaying(5, NarrationContentKind.TextToSpeech);
        var stopped = _manager.MarkStopped(5, "Stopped");
        var idle = _manager.ResetToIdle();

        Assert.Equal(NarrationPlaybackStatus.Stopped, stopped.Status);
        Assert.Equal(NarrationPlaybackStatus.Idle, idle.Status);
        Assert.Null(idle.ActivePoiId);
    }

    private static NarrationRequest CreateRequest(int poiId, int priority, string audioUrl, string text)
    {
        return new NarrationRequest
        {
            PoiId = poiId,
            Priority = priority,
            Title = $"POI {poiId}",
            AudioUrl = audioUrl,
            Text = text
        };
    }

    private sealed class FakeAudioNarrationPlayer(bool canPlay) : IAudioNarrationPlayer
    {
        public bool CanPlay(string audioUrl)
        {
            return canPlay;
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
}
