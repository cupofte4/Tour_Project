using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Tests;

public class OfflineCacheDataServiceTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("{}", true)]
    public void SqliteStallDataCache_HasStoredJson_FiltersBlankPayloadsSafely(string? value, bool expected)
    {
        Assert.Equal(expected, SqliteStallDataCache.HasStoredJson(value));
    }

    [Fact]
    public async Task CachedStallDataService_RefreshStallsAsync_UpdatesLocalCache()
    {
        var apiClient = new FakeNetworkStallApiClient
        {
            Stalls =
            [
                new StallSummary
                {
                    Id = 1,
                    Name = "Oc Co Lan",
                    Category = "Hai san",
                    Latitude = 10.76042,
                    Longitude = 106.69321,
                    TriggerRadiusMeters = 40,
                    Priority = 1,
                    IsActive = true
                }
            ]
        };
        var cache = new FakeStallDataCache();
        var service = new CachedStallDataService(apiClient, cache);

        var refreshed = await service.RefreshStallsAsync();
        var cached = await service.GetCachedStallsAsync();

        Assert.Single(refreshed);
        Assert.Single(cached);
        Assert.Equal("Oc Co Lan", cached[0].Name);
    }

    [Fact]
    public async Task CachedStallDataService_UpdateLocalAudioPathAsync_PersistsToCachedDetail()
    {
        var cache = new FakeStallDataCache();
        await cache.SaveStallDetailAsync(new StallDetail
        {
            Id = 11,
            Name = "Audio Stall",
            DescriptionVi = "Tieng Viet",
            NarrationScriptVi = "Tieng Viet",
            AudioUrl = "https://example.com/audio.m4a",
            Priority = 1,
            IsActive = true
        });

        var service = new CachedStallDataService(new FakeNetworkStallApiClient(), cache);

        await service.UpdateLocalAudioPathAsync(11, "/tmp/stall-11.m4a");
        var cachedDetail = await service.GetCachedStallDetailAsync(11);

        Assert.Equal("/tmp/stall-11.m4a", cachedDetail!.LocalAudioPath);
    }

    [Fact]
    public async Task StallListViewModel_LoadAsync_UsesCachedList_WhenRefreshFails()
    {
        var dataService = new FakeStallDataService
        {
            CachedStalls =
            [
                new StallSummary
                {
                    Id = 2,
                    Name = "Saved POI",
                    Category = "Food",
                    Latitude = 10.76042,
                    Longitude = 106.69321,
                    TriggerRadiusMeters = 40,
                    Priority = 1,
                    IsActive = true
                }
            ],
            RefreshStallsException = new InvalidOperationException("offline")
        };
        var viewModel = new StallListViewModel(dataService, new NoopNarrationService(), new TestSettingsService());

        await viewModel.LoadAsync();

        Assert.Single(viewModel.Stalls);
        Assert.Equal("Saved POI", viewModel.Stalls[0].Name);
        Assert.Equal("Showing saved locations because the latest content couldn't be refreshed.", viewModel.NoticeMessage);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public async Task StallDetailViewModel_LoadAsync_UsesCachedDetail_WhenBackendIsUnavailable()
    {
        var dataService = new FakeStallDataService
        {
            CachedDetail = CreateDetail(3, "Saved Detail", "Saved VI"),
            RefreshDetailException = new InvalidOperationException("offline")
        };
        var viewModel = new StallDetailViewModel(dataService, new RecordingNarrationService(), new TestSettingsService());

        await viewModel.LoadAsync(3);

        Assert.Equal("Saved Detail", viewModel.DisplayName);
        Assert.Equal("Saved VI", viewModel.DisplayDescription);
        Assert.Equal("Showing saved details because the latest content couldn't be refreshed.", viewModel.RefreshNoticeMessage);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public async Task StallDetailViewModel_LoadAsync_UsesCachedTranslation_WhenOffline()
    {
        var dataService = new FakeStallDataService
        {
            CachedDetail = CreateDetail(4, "Quan An", "Tieng Viet"),
            RefreshDetailException = new InvalidOperationException("offline"),
            CachedTranslation = new StallTranslation
            {
                StallId = 4,
                LanguageCode = "de",
                Name = "Deutscher Name",
                Description = "Deutsche Beschreibung"
            },
            TranslationException = new InvalidOperationException("offline")
        };
        var viewModel = new StallDetailViewModel(dataService, new RecordingNarrationService(), new TestSettingsService())
        {
            SelectedLanguage = new LanguageOption { Code = "de", DisplayName = "German" }
        };

        await viewModel.LoadAsync(4);

        Assert.Equal("Deutscher Name", viewModel.DisplayName);
        Assert.Equal("Deutsche Beschreibung", viewModel.DisplayDescription);
        Assert.Equal("de", viewModel.ResolvedDisplayLanguageCode);
        Assert.False(viewModel.HasTranslationError);
    }

    [Fact]
    public async Task StallDetailViewModel_StartNarrationAsync_UsesCachedNarrationMetadata_AndLocalAudioPath()
    {
        var localAudioPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.m4a");
        await File.WriteAllBytesAsync(localAudioPath, [1, 2, 3]);

        try
        {
            var narrationService = new RecordingNarrationService();
            var dataService = new FakeStallDataService
            {
                CachedDetail = new StallDetail
                {
                    Id = 5,
                    Name = "Audio Stall",
                    DescriptionVi = string.Empty,
                    NarrationScriptVi = string.Empty,
                    AudioUrl = "https://example.com/audio.m4a",
                    LocalAudioPath = localAudioPath,
                    Priority = 1,
                    IsActive = true
                },
                RefreshDetailException = new InvalidOperationException("offline")
            };
            var viewModel = new StallDetailViewModel(dataService, narrationService, new TestSettingsService());

            await viewModel.LoadAsync(5);
            await viewModel.StartNarrationAsync();

            Assert.NotNull(narrationService.LastRequest);
            Assert.Equal(localAudioPath, narrationService.LastRequest!.AudioUrl);
            Assert.Equal(string.Empty, narrationService.LastRequest.Text);
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
    public async Task StallDetailViewModel_LoadAsync_ClearsMissingLocalAudioPath_FromCache()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.m4a");
        var dataService = new FakeStallDataService
        {
            CachedDetail = new StallDetail
            {
                Id = 6,
                Name = "Audio Stall",
                DescriptionVi = "Tieng Viet",
                NarrationScriptVi = "Tieng Viet",
                AudioUrl = "https://example.com/audio.m4a",
                LocalAudioPath = missingPath,
                Priority = 1,
                IsActive = true
            },
            RefreshDetailException = new InvalidOperationException("offline")
        };
        var viewModel = new StallDetailViewModel(dataService, new RecordingNarrationService(), new TestSettingsService());

        await viewModel.LoadAsync(6);

        Assert.Equal(string.Empty, viewModel.Stall!.LocalAudioPath);
        Assert.Equal(string.Empty, dataService.LastUpdatedLocalAudioPath);
        Assert.Equal(6, dataService.LastUpdatedStallId);
        Assert.False(viewModel.HasOfflineAudio);
        Assert.Equal("Download for offline use", viewModel.OfflineAudioStatusText);
    }

    [Fact]
    public async Task StallListViewModel_LoadAsync_EmptyCacheAndOffline_UsesCalmFailure()
    {
        var dataService = new FakeStallDataService
        {
            RefreshStallsException = new InvalidOperationException("offline")
        };
        var viewModel = new StallListViewModel(dataService, new NoopNarrationService(), new TestSettingsService());

        await viewModel.LoadAsync();

        Assert.True(viewModel.HasError);
        Assert.Equal("We couldn't load locations right now.", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Stalls);
    }

    private static StallDetail CreateDetail(int id, string name, string narrationScriptVi)
    {
        return new StallDetail
        {
            Id = id,
            Name = name,
            DescriptionVi = narrationScriptVi,
            NarrationScriptVi = narrationScriptVi,
            Priority = 1,
            IsActive = true
        };
    }

    private sealed class FakeStallDataService : IStallDataService
    {
        public IReadOnlyList<StallSummary> CachedStalls { get; init; } = [];

        public IReadOnlyList<StallSummary> RefreshedStalls { get; init; } = [];

        public StallDetail? CachedDetail { get; set; }

        public StallDetail? RefreshedDetail { get; init; }

        public StallTranslation? CachedTranslation { get; init; }

        public StallTranslation? LoadedTranslation { get; init; }

        public Exception? RefreshStallsException { get; init; }

        public Exception? RefreshDetailException { get; init; }

        public Exception? TranslationException { get; init; }

        public int? LastUpdatedStallId { get; private set; }

        public string? LastUpdatedLocalAudioPath { get; private set; }

        public Task<IReadOnlyList<StallSummary>> GetCachedStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CachedStalls);
        }

        public Task<IReadOnlyList<StallSummary>> RefreshStallsAsync(CancellationToken cancellationToken = default)
        {
            if (RefreshStallsException is not null)
            {
                throw RefreshStallsException;
            }

            return Task.FromResult(RefreshedStalls);
        }

        public Task<StallDetail?> GetCachedStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CachedDetail?.Id == stallId ? CachedDetail : null);
        }

        public Task<StallDetail?> RefreshStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
        {
            if (RefreshDetailException is not null)
            {
                throw RefreshDetailException;
            }

            return Task.FromResult(RefreshedDetail?.Id == stallId ? RefreshedDetail : null);
        }

        public Task UpdateLocalAudioPathAsync(int stallId, string? localAudioPath, CancellationToken cancellationToken = default)
        {
            LastUpdatedStallId = stallId;
            LastUpdatedLocalAudioPath = localAudioPath;

            if (CachedDetail is not null && CachedDetail.Id == stallId)
            {
                CachedDetail = new StallDetail
                {
                    Id = CachedDetail.Id,
                    Name = CachedDetail.Name,
                    DescriptionVi = CachedDetail.DescriptionVi,
                    Latitude = CachedDetail.Latitude,
                    Longitude = CachedDetail.Longitude,
                    TriggerRadiusMeters = CachedDetail.TriggerRadiusMeters,
                    Priority = CachedDetail.Priority,
                    OpenHours = CachedDetail.OpenHours,
                    Category = CachedDetail.Category,
                    ImageUrl = CachedDetail.ImageUrl,
                    MapLink = CachedDetail.MapLink,
                    NarrationScriptVi = CachedDetail.NarrationScriptVi,
                    AudioUrl = CachedDetail.AudioUrl,
                    LocalAudioPath = localAudioPath ?? string.Empty,
                    IsActive = CachedDetail.IsActive,
                    AverageRating = CachedDetail.AverageRating,
                    Translations = CachedDetail.Translations
                };
            }

            return Task.CompletedTask;
        }

        public Task<StallTranslation?> GetCachedTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                CachedTranslation is not null &&
                CachedTranslation.StallId == stallId &&
                string.Equals(CachedTranslation.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase)
                    ? CachedTranslation
                    : null);
        }

        public Task<StallTranslation?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            if (TranslationException is not null)
            {
                throw TranslationException;
            }

            return Task.FromResult(
                LoadedTranslation is not null &&
                LoadedTranslation.StallId == stallId &&
                string.Equals(LoadedTranslation.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase)
                    ? LoadedTranslation
                    : null);
        }
    }

    private sealed class FakeStallDataCache : IStallDataCache
    {
        private readonly List<StallSummary> _stalls = [];
        private readonly Dictionary<int, StallDetail> _details = [];
        private readonly Dictionary<string, StallTranslation> _translations = [];

        public Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallSummary>>(_stalls.ToList());
        }

        public Task SaveStallsAsync(IReadOnlyList<StallSummary> stalls, CancellationToken cancellationToken = default)
        {
            _stalls.Clear();
            _stalls.AddRange(stalls);
            return Task.CompletedTask;
        }

        public Task<StallDetail?> GetStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
        {
            _details.TryGetValue(stallId, out var detail);
            return Task.FromResult<StallDetail?>(detail);
        }

        public Task SaveStallDetailAsync(StallDetail stallDetail, CancellationToken cancellationToken = default)
        {
            _details[stallDetail.Id] = stallDetail;
            return Task.CompletedTask;
        }

        public Task UpdateLocalAudioPathAsync(int stallId, string? localAudioPath, CancellationToken cancellationToken = default)
        {
            if (_details.TryGetValue(stallId, out var detail))
            {
                _details[stallId] = new StallDetail
                {
                    Id = detail.Id,
                    Name = detail.Name,
                    DescriptionVi = detail.DescriptionVi,
                    Latitude = detail.Latitude,
                    Longitude = detail.Longitude,
                    TriggerRadiusMeters = detail.TriggerRadiusMeters,
                    Priority = detail.Priority,
                    OpenHours = detail.OpenHours,
                    Category = detail.Category,
                    ImageUrl = detail.ImageUrl,
                    MapLink = detail.MapLink,
                    NarrationScriptVi = detail.NarrationScriptVi,
                    AudioUrl = detail.AudioUrl,
                    LocalAudioPath = localAudioPath ?? string.Empty,
                    IsActive = detail.IsActive,
                    AverageRating = detail.AverageRating,
                    Translations = detail.Translations
                };
            }

            for (var index = 0; index < _stalls.Count; index++)
            {
                if (_stalls[index].Id != stallId)
                {
                    continue;
                }

                var summary = _stalls[index];
                _stalls[index] = new StallSummary
                {
                    Id = summary.Id,
                    Name = summary.Name,
                    DescriptionVi = summary.DescriptionVi,
                    Latitude = summary.Latitude,
                    Longitude = summary.Longitude,
                    TriggerRadiusMeters = summary.TriggerRadiusMeters,
                    Priority = summary.Priority,
                    Category = summary.Category,
                    OpenHours = summary.OpenHours,
                    ImageUrl = summary.ImageUrl,
                    MapLink = summary.MapLink,
                    NarrationScriptVi = summary.NarrationScriptVi,
                    AudioUrl = summary.AudioUrl,
                    LocalAudioPath = localAudioPath ?? string.Empty,
                    IsActive = summary.IsActive,
                    AverageRating = summary.AverageRating,
                    Translations = summary.Translations
                };
            }

            return Task.CompletedTask;
        }

        public Task<StallTranslation?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            _translations.TryGetValue($"{stallId}:{languageCode}", out var translation);
            return Task.FromResult<StallTranslation?>(translation);
        }

        public Task SaveTranslationAsync(StallTranslation translation, CancellationToken cancellationToken = default)
        {
            _translations[$"{translation.StallId}:{translation.LanguageCode}"] = translation;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNetworkStallApiClient : IStallApiClient
    {
        public IReadOnlyList<StallSummary> Stalls { get; init; } = [];

        public StallDetail? Detail { get; init; }

        public StallTranslation? Translation { get; init; }

        public Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Stalls);
        }

        public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Detail);
        }

        public Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Translation);
        }
    }

    private sealed class RecordingNarrationService : INarrationService
    {
        public event NarrationPlaybackStateChangedEventHandler? PlaybackStateChanged;

        public NarrationPlaybackState CurrentState { get; } = NarrationPlaybackState.Idle;

        public bool IsPlaying => false;

        public NarrationRequest? LastRequest { get; private set; }

        public Task RequestNarrationAsync(NarrationRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
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
