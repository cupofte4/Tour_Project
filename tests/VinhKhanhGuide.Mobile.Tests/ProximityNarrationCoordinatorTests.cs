using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class ProximityNarrationCoordinatorTests
{
    [Fact]
    public async Task HandleTriggerAsync_SamePoiInsideCooldown_DoesNotStartNarrationAgain()
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.Parse("2026-04-10T01:00:00Z"));
        var narrationService = new FakeNarrationService();
        var coordinator = CreateCoordinator(narrationService, timeProvider, enabled: true);
        var stall = CreateStall(1, priority: 1);
        var trigger = CreateNotification(stall);

        var firstPrompt = await coordinator.HandleTriggerAsync(trigger, [stall]);
        await narrationService.StopAsync();
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        var secondPrompt = await coordinator.HandleTriggerAsync(trigger, [stall]);

        Assert.NotNull(firstPrompt);
        Assert.NotNull(secondPrompt);
        Assert.Single(narrationService.Requests);
        Assert.True(firstPrompt.AutoPlayStarted);
        Assert.False(secondPrompt.AutoPlayStarted);
        Assert.True(secondPrompt.CanStartNarration);
    }

    [Fact]
    public async Task HandleTriggerAsync_SamePoiWhileAlreadyNarrating_DoesNotDuplicatePlayback()
    {
        var narrationService = new FakeNarrationService
        {
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Playing,
                ActivePoiId = 4
            },
            IsPlaying = true
        };

        var coordinator = CreateCoordinator(narrationService, new FakeTimeProvider(DateTimeOffset.UtcNow), enabled: true);
        var stall = CreateStall(4, priority: 2);

        var prompt = await coordinator.HandleTriggerAsync(CreateNotification(stall), [stall]);

        Assert.NotNull(prompt);
        Assert.Empty(narrationService.Requests);
        Assert.False(prompt.CanStartNarration);
        Assert.Equal("This POI is already playing.", prompt.PromptText);
    }

    [Fact]
    public async Task HandleTriggerAsync_NewPoi_CanReplaceCurrentNarration()
    {
        var narrationService = new FakeNarrationService
        {
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Playing,
                ActivePoiId = 1
            },
            IsPlaying = true
        };

        var coordinator = CreateCoordinator(narrationService, new FakeTimeProvider(DateTimeOffset.UtcNow), enabled: true);
        var replacementStall = CreateStall(2, priority: 10);

        var prompt = await coordinator.HandleTriggerAsync(CreateNotification(replacementStall), [replacementStall]);

        Assert.NotNull(prompt);
        Assert.Single(narrationService.Requests);
        Assert.Equal(2, narrationService.Requests[0].PoiId);
        Assert.True(prompt.AutoPlayStarted);
    }

    [Fact]
    public async Task HandleTriggerAsync_DoesNotRestackPrompt_ForRepeatedSamePrompt()
    {
        var narrationService = new FakeNarrationService();
        var coordinator = CreateCoordinator(narrationService, new FakeTimeProvider(DateTimeOffset.UtcNow), enabled: false);
        var stall = CreateStall(6, priority: 3);

        var firstPrompt = await coordinator.HandleTriggerAsync(CreateNotification(stall), [stall]);
        var secondPrompt = await coordinator.HandleTriggerAsync(CreateNotification(stall), [stall]);

        Assert.NotNull(firstPrompt);
        Assert.Same(firstPrompt, secondPrompt);
        Assert.Empty(narrationService.Requests);
    }

    [Fact]
    public async Task HandleTriggerAsync_AutoPlayDisabled_ShowsPromptWithoutNarration()
    {
        var narrationService = new FakeNarrationService();
        var coordinator = CreateCoordinator(narrationService, new FakeTimeProvider(DateTimeOffset.UtcNow), enabled: false);
        var stall = CreateStall(8, priority: 5);

        var prompt = await coordinator.HandleTriggerAsync(CreateNotification(stall), [stall]);

        Assert.NotNull(prompt);
        Assert.Empty(narrationService.Requests);
        Assert.True(prompt.CanStartNarration);
        Assert.Equal("Nearby POI. Tap to listen or open details.", prompt.PromptText);
        Assert.Equal("vi", prompt.LanguageCode);
        Assert.Equal("Narration for 8", prompt.NarrationText);
    }

    [Theory]
    [InlineData("en", "English narration", "en", "en-US")]
    [InlineData("zh", "中文旁白", "zh", "zh-CN")]
    [InlineData("de", "Deutsche Erzahlung", "de", "de-DE")]
    [InlineData("vi", "Narration for 10", "vi", "vi-VN")]
    public async Task HandleTriggerAsync_UsesSelectedLanguageForPlayback(
        string preferredLanguage,
        string expectedText,
        string expectedLanguageCode,
        string expectedLocaleCode)
    {
        var narrationService = new FakeNarrationService();
        var coordinator = CreateCoordinator(
            narrationService,
            new FakeTimeProvider(DateTimeOffset.UtcNow),
            enabled: true,
            preferredLanguage: preferredLanguage);
        var stall = CreateStall(
            10,
            priority: 1,
            translations:
            [
                new StallTranslation { StallId = 10, LanguageCode = "en", Name = "English Name", Description = "English narration" },
                new StallTranslation { StallId = 10, LanguageCode = "zh", Name = "中文店名", Description = "中文旁白" },
                new StallTranslation { StallId = 10, LanguageCode = "de", Name = "Deutscher Name", Description = "Deutsche Erzahlung" }
            ]);

        var prompt = await coordinator.HandleTriggerAsync(CreateNotification(stall), [stall]);

        Assert.NotNull(prompt);
        Assert.Single(narrationService.Requests);
        Assert.Equal(expectedText, narrationService.Requests[0].Text);
        Assert.Equal(expectedLanguageCode, narrationService.Requests[0].LanguageCode);
        Assert.Equal(expectedLocaleCode, narrationService.Requests[0].LocaleCode);
    }

    [Fact]
    public async Task HandleTriggerAsync_FallsBackToVietnameseOnly_WhenSelectedTranslationMissing()
    {
        var narrationService = new FakeNarrationService();
        var coordinator = CreateCoordinator(
            narrationService,
            new FakeTimeProvider(DateTimeOffset.UtcNow),
            enabled: true,
            preferredLanguage: "zh");
        var stall = CreateStall(
            11,
            priority: 1,
            translations:
            [
                new StallTranslation { StallId = 11, LanguageCode = "en", Name = "English Name", Description = "English narration" }
            ]);

        var prompt = await coordinator.HandleTriggerAsync(CreateNotification(stall), [stall]);

        Assert.NotNull(prompt);
        Assert.Single(narrationService.Requests);
        Assert.Equal("zh", narrationService.Requests[0].RequestedLanguageCode);
        Assert.Equal("vi", narrationService.Requests[0].LanguageCode);
        Assert.Equal("Narration for 11", narrationService.Requests[0].Text);
        Assert.True(narrationService.Requests[0].UsedFallback);
    }

    private static ProximityNarrationCoordinator CreateCoordinator(
        FakeNarrationService narrationService,
        TimeProvider timeProvider,
        bool enabled,
        string preferredLanguage = "")
    {
        return ProximityNarrationCoordinator.CreateForApiClient(
            new FakeStallApiClient(),
            narrationService,
            new FakeSettingsService
            {
                Settings = new AppSettings
                {
                    AutoNarrationEnabled = enabled,
                    PreferredTtsLanguage = preferredLanguage
                }
            },
            Options.Create(new AutoNarrationOptions
            {
                ReplayCooldown = TimeSpan.FromMinutes(3)
            }),
            timeProvider);
    }

    private static StallSummary CreateStall(int id, int priority, IReadOnlyList<StallTranslation>? translations = null)
    {
        return new StallSummary
        {
            Id = id,
            Name = $"POI {id}",
            Category = "POI",
            Latitude = 10.76042,
            Longitude = 106.69321,
            TriggerRadiusMeters = 50,
            Priority = priority,
            IsActive = true,
            NarrationScriptVi = $"Narration for {id}",
            Translations = translations ?? []
        };
    }

    private static NearbyStallNotification CreateNotification(StallSummary stall)
    {
        return new NearbyStallNotification
        {
            StallId = stall.Id,
            StallName = stall.Name,
            Category = stall.Category,
            DistanceMeters = 12,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    private sealed class FakeNarrationService : INarrationService
    {
        public event NarrationPlaybackStateChangedEventHandler? PlaybackStateChanged;

        public NarrationPlaybackState CurrentState { get; set; } = NarrationPlaybackState.Idle;

        public bool IsPlaying { get; set; }

        public List<NarrationRequest> Requests { get; } = [];

        public Task RequestNarrationAsync(NarrationRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            CurrentState = new NarrationPlaybackState
            {
                Status = NarrationPlaybackStatus.Playing,
                ActivePoiId = request.PoiId,
                RequestedLanguageCode = request.RequestedLanguageCode,
                LanguageCode = request.LanguageCode,
                LocaleCode = request.LocaleCode,
                UsedFallback = request.UsedFallback
            };
            IsPlaying = true;
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            CurrentState = NarrationPlaybackState.Idle;
            IsPlaying = false;
            PlaybackStateChanged?.Invoke(CurrentState);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStallApiClient : IStallApiClient
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

    private sealed class FakeSettingsService : ISettingsService
    {
        public event AppSettingsChangedEventHandler? SettingsChanged;

        public AppSettings Settings { get; set; } = new();

        public AppSettings GetSettings()
        {
            return Settings;
        }

        public void SaveSettings(AppSettings settings)
        {
            Settings = settings;
            SettingsChanged?.Invoke(settings);
        }

        public string GetResolvedApiBaseUrl()
        {
            return "http://localhost:5113/";
        }

        public string GetConfiguredApiBaseUrl()
        {
            return string.Empty;
        }
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }

        public void Advance(TimeSpan by)
        {
            _utcNow = _utcNow.Add(by);
        }
    }
}
