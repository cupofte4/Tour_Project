using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Tests;

public class DemoPolishViewModelTests
{
    [Fact]
    public void DemoUiOptions_DebugToolsVisibility_IsCompileTimeControlled()
    {
#if DEBUG
        Assert.True(DemoUiOptions.ShowDebugTools);
#else
        Assert.False(DemoUiOptions.ShowDebugTools);
#endif
    }

    [Fact]
    public async Task StallListViewModel_LoadFailure_UsesCalmErrorCopy()
    {
        var viewModel = StallListViewModel.CreateForApiClient(new ThrowingListApiClient(), new NoopNarrationService(), new TestSettingsService());

        await viewModel.LoadAsync();

        Assert.Equal("We couldn't load locations right now.", viewModel.ErrorMessage);
        Assert.Equal("Locations are unavailable", viewModel.EmptyStateTitle);
        Assert.Equal("Please try again in a moment.", viewModel.EmptyStateMessage);
    }

    [Fact]
    public async Task StallDetailViewModel_TranslationLoading_ExposesCalmStatusCopy()
    {
        var apiClient = new DelayedTranslationApiClient();
        var viewModel = StallDetailViewModel.CreateForApiClient(apiClient, new NoopNarrationService(), new TestSettingsService());
        viewModel.SelectedLanguage = new LanguageOption { Code = "en", DisplayName = "English" };

        var loadTask = viewModel.LoadAsync(1);
        await Task.Delay(20);

        Assert.True(viewModel.IsTranslationLoading);
        Assert.Equal("Loading translation...", viewModel.PageStatusText);
        Assert.Equal("Loading translation...", viewModel.TranslationStatusText);

        apiClient.ReleaseTranslation();
        await loadTask;
    }

    private sealed class ThrowingListApiClient : IStallApiClient
    {
        public Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
            => Task.FromResult<StallDetail?>(null);

        public Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
            => Task.FromResult<StallTranslation?>(null);
    }

    private sealed class DelayedTranslationApiClient : IStallApiClient
    {
        private readonly TaskCompletionSource<StallTranslation?> _translation = new();

        public Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<StallSummary>>([]);

        public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallDetail?>(new StallDetail
            {
                Id = stallId,
                Name = "Quan An",
                DescriptionVi = "Mo ta",
                NarrationScriptVi = "Tieng Viet",
                Priority = 1,
                Translations = []
            });
        }

        public Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
            => _translation.Task;

        public void ReleaseTranslation()
        {
            _translation.TrySetResult(new StallTranslation
            {
                StallId = 1,
                LanguageCode = "en",
                Name = "English Name",
                Description = "English description"
            });
        }
    }

    private sealed class NoopNarrationService : INarrationService
    {
        public event NarrationPlaybackStateChangedEventHandler? PlaybackStateChanged;

        public NarrationPlaybackState CurrentState { get; } = NarrationPlaybackState.Idle;

        public bool IsPlaying => false;

        public Task RequestNarrationAsync(NarrationRequest request, CancellationToken cancellationToken = default)
        {
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }
    }
}
