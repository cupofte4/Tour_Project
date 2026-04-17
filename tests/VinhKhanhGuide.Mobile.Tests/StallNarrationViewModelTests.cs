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

        var viewModel = StallDetailViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());
        viewModel.SelectedLanguage = new LanguageOption { Code = "zh", DisplayName = "Chinese" };

        await viewModel.LoadAsync(7);
        await viewModel.StartNarrationAsync();

        Assert.NotNull(narrationService.LastRequest);
        Assert.Equal("中文店名", viewModel.DisplayName);
        Assert.Equal("中文旁白", viewModel.DisplayDescription);
        Assert.Equal("zh", narrationService.LastRequest!.RequestedLanguageCode);
        Assert.Equal("zh", narrationService.LastRequest!.LanguageCode);
        Assert.Equal("zh-CN", narrationService.LastRequest.LocaleCode);
        Assert.Equal("中文旁白", narrationService.LastRequest.Text);
        Assert.Equal("https://example.com/audio.mp3", narrationService.LastRequest.AudioUrl);
        Assert.True(viewModel.CanStartNarration);
        Assert.Equal("zh", viewModel.ResolvedNarrationLanguageCode);
        Assert.Equal("zh", viewModel.ResolvedDisplayLanguageCode);
        Assert.False(viewModel.IsNarrationUsingFallbackLanguage);
        Assert.False(viewModel.IsUsingFallbackTranslation);
        Assert.False(viewModel.HasTranslationError);
    }

    [Fact]
    public async Task StallDetailViewModel_SelectingEnglish_UpdatesDisplayContent()
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

        var viewModel = StallDetailViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());
        viewModel.SelectedLanguage = new LanguageOption { Code = "en", DisplayName = "English" };

        await viewModel.LoadAsync(7);

        Assert.Equal("English Name", viewModel.DisplayName);
        Assert.Equal("English narration", viewModel.DisplayDescription);
        Assert.Equal("en", viewModel.ResolvedDisplayLanguageCode);
        Assert.False(viewModel.IsUsingFallbackTranslation);
        Assert.False(viewModel.HasTranslationError);
    }

    [Fact]
    public async Task StallDetailViewModel_SelectingGerman_UpdatesDisplayContent()
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
                        LanguageCode = "de",
                        Name = "Deutscher Name",
                        Description = "Deutsche Beschreibung"
                    }
                ])
        };

        var viewModel = StallDetailViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());
        viewModel.SelectedLanguage = new LanguageOption { Code = "de", DisplayName = "German" };

        await viewModel.LoadAsync(7);

        Assert.Equal("Deutscher Name", viewModel.DisplayName);
        Assert.Equal("Deutsche Beschreibung", viewModel.DisplayDescription);
        Assert.Equal("de", viewModel.ResolvedDisplayLanguageCode);
        Assert.False(viewModel.IsUsingFallbackTranslation);
        Assert.False(viewModel.HasTranslationError);
    }

    [Fact]
    public async Task StallDetailViewModel_LoadAsync_FallsBackToVietnamese_WhenSelectedTranslationMissing()
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

        var viewModel = StallDetailViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());
        viewModel.SelectedLanguage = new LanguageOption { Code = "de", DisplayName = "German" };

        await viewModel.LoadAsync(7);

        Assert.Equal("Tieng Viet", viewModel.DisplayDescription);
        Assert.True(viewModel.CanStartNarration);
        Assert.Equal("Ready to listen to narration.", viewModel.NarrationStatusText);
        Assert.Equal("vi", viewModel.ResolvedNarrationLanguageCode);
        Assert.Equal("vi", viewModel.ResolvedDisplayLanguageCode);
        Assert.True(viewModel.IsNarrationUsingFallbackLanguage);
        Assert.True(viewModel.IsUsingFallbackTranslation);
        Assert.False(viewModel.HasTranslationError);
    }

    [Fact]
    public async Task StallDetailViewModel_StartNarrationAsync_PrefersLocalAudio_WhenFileExists()
    {
        var localAudioPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.m4a");
        await File.WriteAllBytesAsync(localAudioPath, [1, 2, 3]);

        try
        {
            var narrationService = new FakeNarrationService();
            var dataService = new FakeDataService(
                new StallDetail
                {
                    Id = 8,
                    Name = "Audio Stall",
                    DescriptionVi = string.Empty,
                    NarrationScriptVi = string.Empty,
                    AudioUrl = "https://example.com/audio.m4a",
                    LocalAudioPath = localAudioPath,
                    Priority = 1,
                    IsActive = true
                });
            var viewModel = new StallDetailViewModel(
                dataService,
                narrationService,
                new TestSettingsService(),
                new FakeOfflineAudioDownloadService());

            await viewModel.LoadAsync(8);
            await viewModel.StartNarrationAsync();

            Assert.Equal(localAudioPath, narrationService.LastRequest!.AudioUrl);
            Assert.True(viewModel.HasOfflineAudio);
            Assert.Equal("Available offline", viewModel.OfflineAudioStatusText);
        }
        finally
        {
            if (File.Exists(localAudioPath))
            {
                File.Delete(localAudioPath);
            }
        }
    }

    [Fact]
    public async Task StallDetailViewModel_StartNarrationAsync_FallsBackToRemoteAudio_WhenLocalFileIsMissing()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.m4a");
        var narrationService = new FakeNarrationService();
        var dataService = new FakeDataService(
            new StallDetail
            {
                Id = 9,
                Name = "Audio Stall",
                DescriptionVi = "Tieng Viet",
                NarrationScriptVi = "Tieng Viet",
                AudioUrl = "https://example.com/audio.m4a",
                LocalAudioPath = missingPath,
                Priority = 1,
                IsActive = true
            });
        var viewModel = new StallDetailViewModel(
            dataService,
            narrationService,
            new TestSettingsService(),
            new FakeOfflineAudioDownloadService());

        await viewModel.LoadAsync(9);
        await viewModel.StartNarrationAsync();

        Assert.Equal("https://example.com/audio.m4a", narrationService.LastRequest!.AudioUrl);
        Assert.Equal("Tieng Viet", narrationService.LastRequest.Text);
        Assert.Equal(string.Empty, viewModel.Stall!.LocalAudioPath);
        Assert.Equal(string.Empty, dataService.LastUpdatedLocalAudioPath);
    }

    [Fact]
    public async Task StallDetailViewModel_OfflineAudioState_ReflectsDownloadAndRemoval()
    {
        var localAudioPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.m4a");
        var narrationService = new FakeNarrationService();
        var downloadService = new FakeOfflineAudioDownloadService
        {
            DownloadResult = OfflineAudioDownloadResult.Success(localAudioPath)
        };
        var dataService = new FakeDataService(
            new StallDetail
            {
                Id = 10,
                Name = "Audio Stall",
                DescriptionVi = "Tieng Viet",
                NarrationScriptVi = "Tieng Viet",
                AudioUrl = "https://example.com/audio.m4a",
                Priority = 1,
                IsActive = true
            });
        var viewModel = new StallDetailViewModel(dataService, narrationService, new TestSettingsService(), downloadService);

        await viewModel.LoadAsync(10);
        Assert.Equal("Download for offline use", viewModel.OfflineAudioStatusText);
        Assert.True(viewModel.CanDownloadOfflineAudio);
        Assert.False(viewModel.CanRemoveOfflineAudio);

        await viewModel.DownloadOfflineAudioAsync();

        Assert.Equal(localAudioPath, viewModel.Stall!.LocalAudioPath);
        Assert.True(viewModel.HasOfflineAudio);
        Assert.Equal("Available offline", viewModel.OfflineAudioStatusText);
        Assert.False(viewModel.CanDownloadOfflineAudio);
        Assert.True(viewModel.CanRemoveOfflineAudio);

        await viewModel.RemoveOfflineAudioAsync();

        Assert.Equal(string.Empty, viewModel.Stall.LocalAudioPath);
        Assert.False(viewModel.HasOfflineAudio);
        Assert.Equal("Download for offline use", viewModel.OfflineAudioStatusText);
        Assert.True(viewModel.CanDownloadOfflineAudio);
        Assert.False(viewModel.CanRemoveOfflineAudio);
    }

    [Fact]
    public async Task StallListViewModel_PreviewNarrationAsync_UsesRequestedLanguage_WhenAvailable()
    {
        var narrationService = new FakeNarrationService();
        var viewModel = StallListViewModel.CreateForApiClient(new FakeStallApiClient(), narrationService, new TestSettingsService());
        viewModel.SelectedPreviewLanguage = new LanguageOption { Code = "de", DisplayName = "German" };

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
                    LanguageCode = "de",
                    Name = "Essensstand",
                    Description = "Deutsche Vorschau"
                }
            ]
        });

        Assert.NotNull(narrationService.LastRequest);
        Assert.Equal("de", narrationService.LastRequest!.RequestedLanguageCode);
        Assert.Equal("de", narrationService.LastRequest!.LanguageCode);
        Assert.Equal("de-DE", narrationService.LastRequest.LocaleCode);
        Assert.Equal("Deutsche Vorschau", narrationService.LastRequest.Text);
        Assert.Equal("https://example.com/audio.mp3", narrationService.LastRequest.AudioUrl);
        Assert.False(narrationService.LastRequest.UsedFallback);
    }

    [Fact]
    public async Task StallDetailViewModel_ChangingLanguage_StopsStaleNarrationForSameStall()
    {
        var narrationService = new FakeNarrationService
        {
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Playing,
                ActivePoiId = 7,
                RequestedLanguageCode = "vi",
                LanguageCode = "vi"
            },
            IsPlaying = true
        };
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

        var viewModel = StallDetailViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());
        viewModel.SelectedLanguage = new LanguageOption { Code = "vi", DisplayName = "Vietnamese" };

        await viewModel.LoadAsync(7);
        viewModel.SelectedLanguage = new LanguageOption { Code = "en", DisplayName = "English" };
        await viewModel.CurrentTranslationTask;

        Assert.Equal(1, narrationService.StopCallCount);
        Assert.Equal("en", viewModel.ResolvedNarrationLanguageCode);
        Assert.Equal("English narration", viewModel.DisplayDescription);
    }

    [Fact]
    public async Task ManualPreviewAndDetailNarration_ResolveLanguageConsistently()
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
                        LanguageCode = "de",
                        Name = "Deutscher Name",
                        Description = "Deutsche Erzahlung"
                    }
                ])
        };

        var detailViewModel = StallDetailViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());
        detailViewModel.SelectedLanguage = new LanguageOption { Code = "de", DisplayName = "German" };
        var listViewModel = StallListViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());
        listViewModel.SelectedPreviewLanguage = new LanguageOption { Code = "de", DisplayName = "German" };

        await detailViewModel.LoadAsync(7);
        await detailViewModel.StartNarrationAsync();
        var detailRequest = narrationService.LastRequest!;

        await listViewModel.PreviewNarrationAsync(new StallSummary
        {
            Id = 7,
            Name = "Quan An",
            Priority = 1,
            NarrationScriptVi = "Tieng Viet",
            Translations =
            [
                new StallTranslation
                {
                    StallId = 7,
                    LanguageCode = "de",
                    Name = "Deutscher Name",
                    Description = "Deutsche Erzahlung"
                }
            ]
        });

        var previewRequest = narrationService.LastRequest!;
        Assert.Equal(detailRequest.RequestedLanguageCode, previewRequest.RequestedLanguageCode);
        Assert.Equal(detailRequest.LanguageCode, previewRequest.LanguageCode);
        Assert.Equal(detailRequest.LocaleCode, previewRequest.LocaleCode);
        Assert.Equal(detailRequest.Text, previewRequest.Text);
    }

    [Fact]
    public async Task StallDetailViewModel_TranslationFetchFailure_SetsErrorState_AndFallsBackDeterministically()
    {
        var narrationService = new FakeNarrationService();
        var apiClient = new FakeStallApiClient
        {
            StallDetail = CreateDetail([]),
            TranslationFailures =
            {
                ["en"] = new InvalidOperationException("boom")
            }
        };

        var viewModel = StallDetailViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());
        viewModel.SelectedLanguage = new LanguageOption { Code = "en", DisplayName = "English" };

        await viewModel.LoadAsync(7);

        Assert.Equal("Quan An", viewModel.DisplayName);
        Assert.Equal("Tieng Viet", viewModel.DisplayDescription);
        Assert.Equal("vi", viewModel.ResolvedDisplayLanguageCode);
        Assert.True(viewModel.IsUsingFallbackTranslation);
        Assert.True(viewModel.HasTranslationError);
        Assert.Equal("This language is not available yet, so Vietnamese is being used.", viewModel.LastTranslationErrorMessage);
    }

    [Fact]
    public async Task StallDetailViewModel_RepeatedLanguageSwitches_DoNotLeaveStaleContent()
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
                    },
                    new StallTranslation
                    {
                        StallId = 7,
                        LanguageCode = "de",
                        Name = "Deutscher Name",
                        Description = "Deutsche Beschreibung"
                    }
                ])
        };

        var viewModel = StallDetailViewModel.CreateForApiClient(apiClient, narrationService, new TestSettingsService());

        await viewModel.LoadAsync(7);

        viewModel.SelectedLanguage = new LanguageOption { Code = "en", DisplayName = "English" };
        await viewModel.CurrentTranslationTask;
        Assert.Equal("English narration", viewModel.DisplayDescription);

        viewModel.SelectedLanguage = new LanguageOption { Code = "de", DisplayName = "German" };
        await viewModel.CurrentTranslationTask;
        Assert.Equal("Deutsche Beschreibung", viewModel.DisplayDescription);
        Assert.Equal("de", viewModel.ResolvedDisplayLanguageCode);

        viewModel.SelectedLanguage = new LanguageOption { Code = "vi", DisplayName = "Vietnamese" };
        await viewModel.CurrentTranslationTask;
        Assert.Equal("Tieng Viet", viewModel.DisplayDescription);
        Assert.Equal("vi", viewModel.ResolvedDisplayLanguageCode);
        Assert.False(viewModel.HasTranslationError);
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

        public Dictionary<string, StallTranslation> TranslationResults { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Exception> TranslationFailures { get; } = new(StringComparer.OrdinalIgnoreCase);

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
            if (TranslationFailures.TryGetValue(languageCode, out var exception))
            {
                throw exception;
            }

            if (TranslationResults.TryGetValue(languageCode, out var loadedTranslation))
            {
                return Task.FromResult<StallTranslation?>(loadedTranslation);
            }

            var translation = StallDetail?.Translations.FirstOrDefault(item =>
                string.Equals(item.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(translation);
        }
    }

    private sealed class FakeNarrationService : INarrationService
    {
        public event NarrationPlaybackStateChangedEventHandler? PlaybackStateChanged;

        public NarrationPlaybackState CurrentState { get; set; } = NarrationPlaybackState.Idle;

        public bool IsPlaying { get; set; }

        public NarrationRequest? LastRequest { get; private set; }

        public int StopCallCount { get; private set; }

        public Task RequestNarrationAsync(NarrationRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Stopped,
                ActivePoiId = request.PoiId,
                RequestedLanguageCode = request.RequestedLanguageCode,
                LanguageCode = request.LanguageCode,
                LocaleCode = request.LocaleCode,
                UsedFallback = request.UsedFallback,
                Message = "Narration stopped."
            };
            IsPlaying = false;
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            StopCallCount++;
            CurrentState = NarrationPlaybackState.Idle;
            IsPlaying = false;
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDataService : IStallDataService
    {
        public FakeDataService(StallDetail detail)
        {
            Detail = detail;
        }

        public StallDetail Detail { get; private set; }

        public string? LastUpdatedLocalAudioPath { get; private set; }

        public Task<IReadOnlyList<StallSummary>> GetCachedStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallSummary>>([]);
        }

        public Task<IReadOnlyList<StallSummary>> RefreshStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallSummary>>([]);
        }

        public Task<StallDetail?> GetCachedStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallDetail?>(Detail.Id == stallId ? Detail : null);
        }

        public Task<StallDetail?> RefreshStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallDetail?>(Detail.Id == stallId ? Detail : null);
        }

        public Task UpdateLocalAudioPathAsync(int stallId, string? localAudioPath, CancellationToken cancellationToken = default)
        {
            LastUpdatedLocalAudioPath = localAudioPath;
            Detail = new StallDetail
            {
                Id = Detail.Id,
                Name = Detail.Name,
                DescriptionVi = Detail.DescriptionVi,
                Latitude = Detail.Latitude,
                Longitude = Detail.Longitude,
                TriggerRadiusMeters = Detail.TriggerRadiusMeters,
                Priority = Detail.Priority,
                OpenHours = Detail.OpenHours,
                Category = Detail.Category,
                ImageUrl = Detail.ImageUrl,
                MapLink = Detail.MapLink,
                NarrationScriptVi = Detail.NarrationScriptVi,
                AudioUrl = Detail.AudioUrl,
                LocalAudioPath = localAudioPath ?? string.Empty,
                IsActive = Detail.IsActive,
                AverageRating = Detail.AverageRating,
                Translations = Detail.Translations
            };
            return Task.CompletedTask;
        }

        public Task<StallTranslation?> GetCachedTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallTranslation?>(null);
        }

        public Task<StallTranslation?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallTranslation?>(null);
        }
    }

    private sealed class FakeOfflineAudioDownloadService : IOfflineAudioDownloadService
    {
        public OfflineAudioDownloadResult DownloadResult { get; set; } = OfflineAudioDownloadResult.Failure("We couldn't download offline audio right now.");

        public bool CanDownloadAudio(StallDetail? stallDetail)
        {
            return stallDetail is not null && !string.IsNullOrWhiteSpace(stallDetail.AudioUrl);
        }

        public string GetDeterministicLocalPath(StallDetail stallDetail)
        {
            return DownloadResult.LocalAudioPath;
        }

        public async Task<OfflineAudioDownloadResult> DownloadAsync(StallDetail stallDetail, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(DownloadResult.LocalAudioPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DownloadResult.LocalAudioPath)!);
                await File.WriteAllBytesAsync(DownloadResult.LocalAudioPath, [1, 2, 3], cancellationToken);
            }

            return DownloadResult;
        }

        public Task<bool> RemoveAsync(string? localAudioPath, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(localAudioPath) && File.Exists(localAudioPath))
            {
                File.Delete(localAudioPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
