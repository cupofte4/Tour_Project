using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Tests;

public class StallNarrationViewModelTests
{
    [Fact]
    public async Task StallDetailViewModel_StartNarrationAsync_UsesResolvedLanguageText()
    {
        var narrationService = new FakeNarrationService();
        var apiClient = new FakeStallApiClient
        {
            StallDetail = CreateDetail(
                translations:
                [
                    new StallTranslation
                    {
                        StallId = 7,
                        LanguageCode = "zh",
                        Name = "中文店名",
                        Description = "中文旁白"
                    }
                ])
        };

        var viewModel = new StallDetailViewModel(apiClient, narrationService)
        {
            SelectedLanguage = new LanguageOption { Code = "zh", DisplayName = "Chinese" }
        };

        await viewModel.LoadAsync(7);
        await viewModel.StartNarrationAsync();

        Assert.NotNull(narrationService.LastRequest);
        Assert.Equal("zh", narrationService.LastRequest!.LanguageCode);
        Assert.Equal("中文旁白", narrationService.LastRequest.Text);
        Assert.Equal(string.Empty, narrationService.LastRequest.AudioUrl);
        Assert.True(viewModel.CanStartNarration);
    }

    [Fact]
    public async Task StallDetailViewModel_LoadAsync_FallsBackToEnglish_WhenSelectedTranslationMissing()
    {
        var narrationService = new FakeNarrationService();
        var apiClient = new FakeStallApiClient
        {
            StallDetail = CreateDetail(
                translations:
                [
                    new StallTranslation
                    {
                        StallId = 7,
                        LanguageCode = "en",
                        Name = "English Name",
                        Description = "English narration"
                    }
                ])
        };

        var viewModel = new StallDetailViewModel(apiClient, narrationService)
        {
            SelectedLanguage = new LanguageOption { Code = "de", DisplayName = "German" }
        };

        await viewModel.LoadAsync(7);

        Assert.Equal("English narration", viewModel.DisplayDescription);
        Assert.True(viewModel.CanStartNarration);
        Assert.Equal("Ready to play narration.", viewModel.NarrationStatusText);
    }

    [Fact]
    public async Task StallListViewModel_PreviewNarrationAsync_UsesSameFallbackResolver()
    {
        var narrationService = new FakeNarrationService();
        var viewModel = new StallListViewModel(new FakeStallApiClient(), narrationService)
        {
            SelectedPreviewLanguage = new LanguageOption { Code = "de", DisplayName = "German" }
        };

        await viewModel.PreviewNarrationAsync(new StallSummary
        {
            Id = 4,
            Name = "Quan An",
            Priority = 1,
            AudioUrl = "https://example.com/audio.mp3",
            NarrationScriptVi = "Tieng Viet",
            Translations =
            [
                new StallTranslation
                {
                    StallId = 4,
                    LanguageCode = "en",
                    Name = "Food Stall",
                    Description = "English preview"
                }
            ]
        });

        Assert.NotNull(narrationService.LastRequest);
        Assert.Equal("en", narrationService.LastRequest!.LanguageCode);
        Assert.Equal("English preview", narrationService.LastRequest.Text);
        Assert.Equal(string.Empty, narrationService.LastRequest.AudioUrl);
    }

    private static StallDetail CreateDetail(IReadOnlyList<StallTranslation> translations)
    {
        return new StallDetail
        {
            Id = 7,
            Name = "Quan An",
            DescriptionVi = "Mo ta tieng Viet",
            NarrationScriptVi = "Tieng Viet",
            AudioUrl = "https://example.com/audio.mp3",
            Priority = 1,
            Translations = translations
        };
    }

    private sealed class FakeStallApiClient : IStallApiClient
    {
        public StallDetail? StallDetail { get; init; }

        public Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallSummary>>([]);
        }

        public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StallDetail);
        }

        public Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            var translation = StallDetail?.Translations.FirstOrDefault(item =>
                string.Equals(item.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(translation);
        }
    }

    private sealed class FakeNarrationService : INarrationService
    {
        public event NarrationPlaybackStateChangedEventHandler? PlaybackStateChanged;

        public NarrationPlaybackState CurrentState { get; private set; } = NarrationPlaybackState.Idle;

        public bool IsPlaying => CurrentState.Status is NarrationPlaybackStatus.Queued
            or NarrationPlaybackStatus.Preparing
            or NarrationPlaybackStatus.Playing;

        public NarrationRequest? LastRequest { get; private set; }

        public Task RequestNarrationAsync(NarrationRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Stopped,
                ActivePoiId = request.PoiId,
                Message = "Narration stopped."
            };
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            CurrentState = NarrationPlaybackState.Idle;
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }
    }
}
