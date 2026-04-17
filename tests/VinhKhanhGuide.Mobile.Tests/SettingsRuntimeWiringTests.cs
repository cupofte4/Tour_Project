using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Tests;

public class SettingsRuntimeWiringTests
{
    [Fact]
    public async Task StallDetailViewModel_LoadAsync_UsesPersistedPreferredLanguage_WhenNoExplicitOverrideExists()
    {
        var settingsService = new TestSettingsService();
        settingsService.SaveSettings(new AppSettings
        {
            PreferredTtsLanguage = "de"
        });

        var viewModel = StallDetailViewModel.CreateForApiClient(
            new PreferredLanguageApiClient(),
            new NoopNarrationService(),
            settingsService);

        await viewModel.LoadAsync(7);

        Assert.Equal("de", viewModel.SelectedLanguage?.Code);
        Assert.Equal("de", viewModel.RequestedNarrationLanguageCode);
        Assert.Equal("de", viewModel.ResolvedNarrationLanguageCode);
        Assert.Equal("Deutscher Name", viewModel.DisplayName);
        Assert.Equal("Deutsche Beschreibung", viewModel.DisplayDescription);
    }

    [Fact]
    public async Task StallDetailViewModel_LoadAsync_ExplicitLanguageOverride_WinsOverPersistedPreference()
    {
        var settingsService = new TestSettingsService();
        settingsService.SaveSettings(new AppSettings
        {
            PreferredTtsLanguage = "de"
        });

        var viewModel = StallDetailViewModel.CreateForApiClient(
            new PreferredLanguageApiClient(),
            new NoopNarrationService(),
            settingsService);

        await viewModel.LoadAsync(7, "en");

        Assert.Equal("en", viewModel.SelectedLanguage?.Code);
        Assert.Equal("en", viewModel.RequestedNarrationLanguageCode);
        Assert.Equal("en", viewModel.ResolvedNarrationLanguageCode);
        Assert.Equal("English Name", viewModel.DisplayName);
        Assert.Equal("English description", viewModel.DisplayDescription);
    }

    [Fact]
    public void StallListViewModel_UsesPersistedPreferredLanguage_ForPreviewDefault()
    {
        var settingsService = new TestSettingsService();
        settingsService.SaveSettings(new AppSettings
        {
            PreferredTtsLanguage = "de"
        });

        var viewModel = StallListViewModel.CreateForApiClient(
            new EmptyStallApiClient(),
            new NoopNarrationService(),
            settingsService);

        Assert.Equal("de", viewModel.SelectedPreviewLanguage?.Code);
    }

    [Fact]
    public async Task StallMapViewModel_DisablingGpsSetting_StopsTrackingAndClearsNearbyState()
    {
        var settingsService = new TestSettingsService();
        var locationTrackingService = new FakeLocationTrackingService();
        var proximityService = new FakeProximityService();
        var proximityCoordinator = new FakeProximityNarrationCoordinator();
        var viewModel = StallMapViewModel.CreateForApiClient(
            new EmptyStallApiClient(),
            new FakeDistanceCalculator(),
            locationTrackingService,
            proximityService,
            proximityCoordinator,
            settingsService);

        viewModel.Stalls.Add(new StallSummary
        {
            Id = 1,
            Name = "POI 1",
            Category = "Food",
            Latitude = 10.76042,
            Longitude = 106.69321,
            TriggerRadiusMeters = 50,
            Priority = 1,
            IsActive = true,
            NarrationScriptVi = "Tieng Viet"
        });

        await viewModel.StartLocationTrackingAsync();

        Assert.True(viewModel.IsTracking);
        Assert.NotNull(viewModel.UserLocation);
        Assert.NotNull(viewModel.NearbyStall);

        settingsService.SaveSettings(new AppSettings
        {
            IsGpsTrackingEnabled = false
        });

        await Task.Delay(50);

        Assert.False(viewModel.IsTracking);
        Assert.Null(viewModel.UserLocation);
        Assert.Null(viewModel.NearbyStall);
        Assert.Null(viewModel.NearestStall);
        Assert.Equal(string.Empty, viewModel.NearestStallDistanceText);
        Assert.True(locationTrackingService.StopCallCount > 0);
        Assert.True(proximityService.ResetCallCount > 0);
        Assert.Contains("turned off", viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SettingsPage_DoesNotExpose_UnsupportedPreferredVoiceIdControl()
    {
        var settingsPagePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/mobile/VinhKhanhGuide.Mobile/Views/SettingsPage.xaml"));

        var xaml = File.ReadAllText(settingsPagePath);

        Assert.DoesNotContain("Preferred Voice ID", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsPage_ExposesApiBaseUrlOverrideField()
    {
        var settingsPagePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/mobile/VinhKhanhGuide.Mobile/Views/SettingsPage.xaml"));

        var xaml = File.ReadAllText(settingsPagePath);

        Assert.Contains("API Base URL Override", xaml, StringComparison.Ordinal);
    }

    private sealed class PreferredLanguageApiClient : IStallApiClient
    {
        public Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallSummary>>([]);
        }

        public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallDetail?>(new StallDetail
            {
                Id = stallId,
                Name = "Quan An",
                DescriptionVi = "Mo ta tieng Viet",
                NarrationScriptVi = "Tieng Viet",
                Priority = 1,
                Translations =
                [
                    new StallTranslation
                    {
                        StallId = stallId,
                        LanguageCode = "en",
                        Name = "English Name",
                        Description = "English description"
                    },
                    new StallTranslation
                    {
                        StallId = stallId,
                        LanguageCode = "de",
                        Name = "Deutscher Name",
                        Description = "Deutsche Beschreibung"
                    }
                ]
            });
        }

        public Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallTranslation?>(null);
        }
    }

    private sealed class EmptyStallApiClient : IStallApiClient
    {
        public Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallSummary>>([]);
        }

        public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallDetail?>(null);
        }

        public Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallTranslation?>(null);
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

    private sealed class FakeLocationTrackingService : ILocationTrackingService
    {
#pragma warning disable CS0067
        public event LocationUpdatedEventHandler? LocationUpdated;
#pragma warning restore CS0067

        public bool IsTracking { get; private set; }

        public int StopCallCount { get; private set; }

        public Task<LocationResult> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateReadyLocationResult());
        }

        public Task<LocationResult> StartTrackingAsync(
            LocationTrackingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            IsTracking = true;
            return Task.FromResult(CreateReadyLocationResult());
        }

        public Task StopTrackingAsync()
        {
            IsTracking = false;
            StopCallCount++;
            return Task.CompletedTask;
        }

        private static LocationResult CreateReadyLocationResult()
        {
            return new LocationResult
            {
                CurrentLocation = new GeoPoint(10.76042, 106.69321),
                Status = LocationTrackingStatus.Ready
            };
        }
    }

    private sealed class FakeProximityService : IProximityService
    {
        public int ResetCallCount { get; private set; }

        public NearbyStallNotification? EvaluateNearbyStall(GeoPoint currentLocation, IEnumerable<StallSummary> stalls, DateTimeOffset now)
        {
            return ProcessLocationUpdate(currentLocation, stalls, now);
        }

        public NearbyStallNotification? ProcessLocationUpdate(GeoPoint currentLocation, IEnumerable<StallSummary> stalls, DateTimeOffset now)
        {
            var stall = stalls.First();
            return new NearbyStallNotification
            {
                StallId = stall.Id,
                StallName = stall.Name,
                Category = stall.Category,
                DistanceMeters = 10,
                Timestamp = now
            };
        }

        public void Reset()
        {
            ResetCallCount++;
        }
    }

    private sealed class FakeProximityNarrationCoordinator : IProximityNarrationCoordinator
    {
        public NearbyStallNotification? CurrentPrompt { get; private set; }

        public NearbyStallNotification? DismissPrompt()
        {
            CurrentPrompt = null;
            return null;
        }

        public Task<NearbyStallNotification?> HandleTriggerAsync(
            NearbyStallNotification notification,
            IEnumerable<StallSummary> stalls,
            CancellationToken cancellationToken = default)
        {
            CurrentPrompt = new NearbyStallNotification
            {
                StallId = notification.StallId,
                StallName = notification.StallName,
                Category = notification.Category,
                ImageUrl = notification.ImageUrl,
                DistanceMeters = notification.DistanceMeters,
                TriggerReason = notification.TriggerReason,
                Timestamp = notification.Timestamp,
                CanStartNarration = true,
                PromptText = "Nearby POI. Tap to listen or open details."
            };

            return Task.FromResult<NearbyStallNotification?>(CurrentPrompt);
        }

        public Task<NearbyStallNotification?> StartPromptNarrationAsync(
            IEnumerable<StallSummary> stalls,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<NearbyStallNotification?>(CurrentPrompt);
        }
    }

    private sealed class FakeDistanceCalculator : IProximityDistanceCalculator
    {
        public double CalculateMeters(GeoPoint currentLocation, GeoPoint destination)
        {
            return 10;
        }
    }
}
