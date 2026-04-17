using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class ExternalEntryServiceTests
{
    [Fact]
    public async Task ProcessAsync_ValidPayload_CreatesNavigationIntent()
    {
        var navigator = new FakeNavigator();
        var timeProvider = new TestTimeProvider();
        var service = ExternalEntryService.CreateForApiClient(
            new ExternalStallRouteParser(),
            new FakeStallApiClient(CreateDetail()),
            navigator,
            new ExternalEntryStatusService(),
            timeProvider);

        var result = await service.ProcessAsync(
            "vinhkhanhguide://stall/7?autoplay=1&lang=de",
            ExternalEntrySourceType.DeepLink);

        Assert.True(result.Succeeded);
        Assert.Equal(ExternalEntryOutcome.Opened, result.Outcome);
        Assert.NotNull(navigator.LastRoute);
        Assert.Equal(7, navigator.LastRoute!.StallId);
        Assert.True(navigator.LastRoute.AutoPlay);
        Assert.Equal("de", navigator.LastRoute.RequestedLanguageCode);
    }

    [Fact]
    public async Task ProcessAsync_InvalidPayload_ReturnsCalmFailureWithoutNavigation()
    {
        var navigator = new FakeNavigator();
        var service = ExternalEntryService.CreateForApiClient(
            new ExternalStallRouteParser(),
            new FakeStallApiClient(CreateDetail()),
            navigator,
            new ExternalEntryStatusService(),
            new TestTimeProvider());

        var result = await service.ProcessAsync("stall:abc", ExternalEntrySourceType.Manual);

        Assert.False(result.Succeeded);
        Assert.Equal(ExternalEntryOutcome.InvalidPayload, result.Outcome);
        Assert.Equal("That code could not be opened.", result.UserMessage);
        Assert.Null(navigator.LastRoute);
    }

    [Fact]
    public async Task ProcessAsync_UnsupportedRoute_ReturnsCalmFailureWithoutNavigation()
    {
        var navigator = new FakeNavigator();
        var service = ExternalEntryService.CreateForApiClient(
            new ExternalStallRouteParser(),
            new FakeStallApiClient(CreateDetail()),
            navigator,
            new ExternalEntryStatusService(),
            new TestTimeProvider());

        var result = await service.ProcessAsync("vinhkhanhguide://map/99", ExternalEntrySourceType.DeepLink);

        Assert.False(result.Succeeded);
        Assert.Equal(ExternalEntryOutcome.UnsupportedRoute, result.Outcome);
        Assert.Equal("That link is not supported.", result.UserMessage);
        Assert.Null(navigator.LastRoute);
    }

    [Fact]
    public async Task ProcessAsync_StallNotFound_FailsGracefullyWithoutNavigation()
    {
        var navigator = new FakeNavigator();
        var service = ExternalEntryService.CreateForApiClient(
            new ExternalStallRouteParser(),
            new FakeStallApiClient(detail: null),
            navigator,
            new ExternalEntryStatusService(),
            new TestTimeProvider());

        var result = await service.ProcessAsync("stall:99", ExternalEntrySourceType.Qr);

        Assert.False(result.Succeeded);
        Assert.Equal(ExternalEntryOutcome.StallNotFound, result.Outcome);
        Assert.Equal("We couldn't find that location.", result.UserMessage);
        Assert.Null(navigator.LastRoute);
    }

    [Fact]
    public async Task ProcessAsync_RepeatedIdenticalPayloadWithinCooldown_DoesNotNavigateTwice()
    {
        var navigator = new FakeNavigator();
        var timeProvider = new TestTimeProvider();
        var service = ExternalEntryService.CreateForApiClient(
            new ExternalStallRouteParser(),
            new FakeStallApiClient(CreateDetail()),
            navigator,
            new ExternalEntryStatusService(),
            timeProvider);

        var first = await service.ProcessAsync("stall:7", ExternalEntrySourceType.Qr);
        var duplicate = await service.ProcessAsync("stall:7", ExternalEntrySourceType.Qr);

        Assert.True(first.Succeeded);
        Assert.True(duplicate.Succeeded);
        Assert.Equal(ExternalEntryOutcome.IgnoredDuplicate, duplicate.Outcome);
        Assert.True(duplicate.IsDuplicate);
        Assert.Equal(1, navigator.NavigateCallCount);
    }

    [Fact]
    public async Task ProcessAsync_RepeatedIdenticalPayloadAfterCooldown_NavigatesAgain()
    {
        var navigator = new FakeNavigator();
        var timeProvider = new TestTimeProvider();
        var service = ExternalEntryService.CreateForApiClient(
            new ExternalStallRouteParser(),
            new FakeStallApiClient(CreateDetail()),
            navigator,
            new ExternalEntryStatusService(),
            timeProvider);

        await service.ProcessAsync("vinhkhanhguide://stall/7", ExternalEntrySourceType.DeepLink);
        timeProvider.Advance(TimeSpan.FromSeconds(3));
        var second = await service.ProcessAsync("vinhkhanhguide://stall/7", ExternalEntrySourceType.DeepLink);

        Assert.True(second.Succeeded);
        Assert.Equal(ExternalEntryOutcome.Opened, second.Outcome);
        Assert.Equal(2, navigator.NavigateCallCount);
    }

    private static StallDetail CreateDetail()
    {
        return new StallDetail
        {
            Id = 7,
            Name = "Stall 7",
            DescriptionVi = "Mo ta",
            NarrationScriptVi = "Xin chao",
            Priority = 1
        };
    }

    private sealed class FakeNavigator : IAppNavigator
    {
        public ExternalStallRoute? LastRoute { get; private set; }
        public int NavigateCallCount { get; private set; }

        public Task NavigateToStallDetailAsync(ExternalStallRoute route)
        {
            LastRoute = route;
            NavigateCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStallApiClient : IStallApiClient
    {
        private readonly StallDetail? _detail;

        public FakeStallApiClient(StallDetail? detail)
        {
            _detail = detail;
        }

        public Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallSummary>>([]);
        }

        public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_detail is not null && _detail.Id == stallId ? _detail : null);
        }

        public Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallTranslation?>(null);
        }
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow = new(2026, 04, 17, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }

        public void Advance(TimeSpan duration)
        {
            _utcNow = _utcNow.Add(duration);
        }
    }
}
