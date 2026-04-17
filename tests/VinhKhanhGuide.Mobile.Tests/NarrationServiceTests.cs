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
            RequestedLanguageCode = "en",
            LanguageCode = "en"
        });

        await tts.WaitForInvocationAsync();

        Assert.True(service.IsPlaying);
        Assert.Equal(NarrationPlaybackStatus.Playing, service.CurrentState.Status);
        Assert.Equal(9, service.CurrentState.ActivePoiId);
        Assert.Equal("en", service.CurrentState.RequestedLanguageCode);
        Assert.Equal("en", service.CurrentState.LanguageCode);
        Assert.Equal("en", tts.LanguageCode);
        Assert.Equal(string.Empty, tts.LocaleCode);

        tts.Release();
        await requestTask;
        Assert.Equal(NarrationPlaybackStatus.Idle, service.CurrentState.Status);
    }

    [Fact]
    public async Task RequestNarrationAsync_PrefersTextToSpeechForChinese_WhenAudioIsAvailable()
    {
        var tts = new FakeDeviceTextToSpeech();
        var audioPlayer = new FakeAudioNarrationPlayer(canPlay: true);
        var service = new NarrationService(
            new NarrationSessionManager(),
            tts,
            audioPlayer);

        var requestTask = service.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = 12,
            Priority = 1,
            Title = "POI 12",
            AudioUrl = "https://example.com/audio.mp3",
            Text = "中文旁白",
            RequestedLanguageCode = "zh",
            LanguageCode = "zh",
            LocaleCode = "zh-CN"
        });

        await tts.WaitForInvocationAsync();

        Assert.Equal(0, audioPlayer.PlayCallCount);
        Assert.Equal("zh", tts.LanguageCode);
        Assert.Equal("zh-CN", tts.LocaleCode);

        tts.Release();
        await requestTask;
    }

    [Fact]
    public async Task RequestNarrationAsync_UsesAudioForVietnamese_WhenAudioIsAvailable()
    {
        var tts = new FakeDeviceTextToSpeech();
        var audioPlayer = new FakeAudioNarrationPlayer(canPlay: true);
        var service = new NarrationService(
            new NarrationSessionManager(),
            tts,
            audioPlayer);

        await service.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = 13,
            Priority = 1,
            Title = "POI 13",
            AudioUrl = "https://example.com/audio.mp3",
            Text = "Tieng Viet",
            RequestedLanguageCode = "vi",
            LanguageCode = "vi",
            LocaleCode = "vi-VN"
        });

        Assert.Equal(1, audioPlayer.PlayCallCount);
        Assert.Null(tts.LanguageCode);
    }

    [Fact]
    public async Task RequestNarrationAsync_SurfacesLocaleDiscoveryFailureMessage()
    {
        var tts = new FakeDeviceTextToSpeech
        {
            ExceptionToThrow = new TextToSpeechLocaleDiscoveryException("Text-to-speech voices couldn't be discovered on this device.")
        };
        var service = new NarrationService(
            new NarrationSessionManager(),
            tts,
            new FakeAudioNarrationPlayer(canPlay: false));

        await service.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = 14,
            Priority = 1,
            Title = "POI 14",
            Text = "Tieng Viet",
            RequestedLanguageCode = "vi",
            LanguageCode = "vi",
            LocaleCode = "vi-VN"
        });

        Assert.Equal(NarrationPlaybackStatus.Error, service.CurrentState.Status);
        Assert.Equal("Text-to-speech voices couldn't be discovered on this device.", service.CurrentState.Message);
    }

    [Fact]
    public async Task RequestNarrationAsync_SurfacesNoVoiceAvailableMessage()
    {
        var tts = new FakeDeviceTextToSpeech
        {
            ExceptionToThrow = new TextToSpeechLocaleUnavailableException("No Vietnamese text-to-speech voice is available on this device.")
        };
        var service = new NarrationService(
            new NarrationSessionManager(),
            tts,
            new FakeAudioNarrationPlayer(canPlay: false));

        await service.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = 15,
            Priority = 1,
            Title = "POI 15",
            Text = "Tieng Viet",
            RequestedLanguageCode = "vi",
            LanguageCode = "vi",
            LocaleCode = "vi-VN"
        });

        Assert.Equal(NarrationPlaybackStatus.Error, service.CurrentState.Status);
        Assert.Equal("No Vietnamese text-to-speech voice is available on this device.", service.CurrentState.Message);
    }

    private sealed class FakeDeviceTextToSpeech : IDeviceTextToSpeech
    {
        private readonly TaskCompletionSource _invoked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string? LanguageCode { get; private set; }
        public string? LocaleCode { get; private set; }
        public Exception? ExceptionToThrow { get; init; }

        public async Task SpeakAsync(string text, string? languageCode, string? localeCode, CancellationToken cancellationToken)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            LanguageCode = languageCode;
            LocaleCode = localeCode;
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
        public int PlayCallCount { get; private set; }

        public bool CanPlay(string audioUrl)
        {
            return canPlay;
        }

        public Task PlayAsync(string audioUrl, CancellationToken cancellationToken)
        {
            PlayCallCount++;
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
