using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class NarrationServiceTests
{
    [Fact]
    public async Task RequestNarrationAsync_TransitionsToPlaying_ForTtsFallback()
    {
        var tts = new FakeDeviceTextToSpeech();
        var service = new NarrationService(
            new NarrationSessionManager(),
            tts,
            new FakeAudioNarrationPlayer(canPlay: false));

        var requestTask = service.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = 9,
            Priority = 1,
            Title = "POI 9",
            AudioUrl = "https://example.com/audio.mp3",
            Text = "Fallback narration text",
            LanguageCode = "en"
        });

        await tts.WaitForInvocationAsync();

        Assert.True(service.IsPlaying);
        Assert.Equal(NarrationPlaybackStatus.Playing, service.CurrentState.Status);
        Assert.Equal(9, service.CurrentState.ActivePoiId);
        Assert.Equal("en", tts.LanguageCode);

        tts.Release();
        await requestTask;
        Assert.Equal(NarrationPlaybackStatus.Idle, service.CurrentState.Status);
    }

    private sealed class FakeDeviceTextToSpeech : IDeviceTextToSpeech
    {
        private readonly TaskCompletionSource _invoked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string? LanguageCode { get; private set; }

        public async Task SpeakAsync(string text, string? languageCode, CancellationToken cancellationToken)
        {
            LanguageCode = languageCode;
            _invoked.TrySetResult();
            await _release.Task.WaitAsync(cancellationToken);
        }

        public Task WaitForInvocationAsync()
        {
            return _invoked.Task;
        }

        public void Release()
        {
            _release.TrySetResult();
        }
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
