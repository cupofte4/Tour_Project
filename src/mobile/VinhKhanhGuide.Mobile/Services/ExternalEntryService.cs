using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class ExternalEntryService : IExternalEntryService
{
    private static readonly TimeSpan DuplicateCooldown = TimeSpan.FromSeconds(2);

    private readonly IExternalStallRouteParser _parser;
    private readonly IStallDataService _stallDataService;
    private readonly IAppNavigator _navigator;
    private readonly ExternalEntryStatusService _statusService;
    private readonly TimeProvider _timeProvider;
    private readonly object _syncRoot = new();
    private string _lastNavigationKey = string.Empty;
    private DateTimeOffset _lastNavigationAt = DateTimeOffset.MinValue;

    public static ExternalEntryService CreateForApiClient(
        IExternalStallRouteParser parser,
        IStallApiClient stallApiClient,
        IAppNavigator navigator,
        ExternalEntryStatusService statusService,
        TimeProvider timeProvider)
    {
        return new ExternalEntryService(
            parser,
            new DirectStallDataService(stallApiClient),
            navigator,
            statusService,
            timeProvider);
    }

    public ExternalEntryService(
        IExternalStallRouteParser parser,
        IStallDataService stallDataService,
        IAppNavigator navigator,
        ExternalEntryStatusService statusService,
        TimeProvider timeProvider)
    {
        _parser = parser;
        _stallDataService = stallDataService;
        _navigator = navigator;
        _statusService = statusService;
        _timeProvider = timeProvider;
    }

    public async Task<ExternalEntryProcessResult> ProcessAsync(
        string? rawPayload,
        ExternalEntrySourceType sourceType,
        CancellationToken cancellationToken = default)
    {
        var route = _parser.Parse(rawPayload, sourceType);
        if (!route.IsValid)
        {
            var invalidResult = CreateInvalidResult(route);
            await _statusService.ShowErrorAsync(invalidResult.UserMessage);
            return invalidResult;
        }

        try
        {
            var stall = await _stallDataService.GetCachedStallDetailAsync(route.StallId, cancellationToken);
            stall ??= await _stallDataService.RefreshStallDetailAsync(route.StallId, cancellationToken);

            if (stall is null)
            {
                var notFoundResult = new ExternalEntryProcessResult
                {
                    Outcome = ExternalEntryOutcome.StallNotFound,
                    Route = route,
                    UserMessage = "We couldn't find that location."
                };
                await _statusService.ShowErrorAsync(notFoundResult.UserMessage);
                return notFoundResult;
            }

            if (ShouldIgnoreDuplicate(route))
            {
                var duplicateResult = new ExternalEntryProcessResult
                {
                    Succeeded = true,
                    Outcome = ExternalEntryOutcome.IgnoredDuplicate,
                    Route = route,
                    UserMessage = "That location is already opening."
                };
                await _statusService.ShowInfoAsync(duplicateResult.UserMessage, TimeSpan.FromSeconds(1.4));
                return duplicateResult;
            }

            RememberNavigation(route);
            await _statusService.ShowInfoAsync("Opening location...");
            await _navigator.NavigateToStallDetailAsync(route);

            return new ExternalEntryProcessResult
            {
                Succeeded = true,
                Outcome = ExternalEntryOutcome.Opened,
                Route = route,
                UserMessage = "Opening location..."
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            var cachedStall = await _stallDataService.GetCachedStallDetailAsync(route.StallId, cancellationToken);
            if (cachedStall is not null)
            {
                RememberNavigation(route);
                await _statusService.ShowInfoAsync("Opening saved location...");
                await _navigator.NavigateToStallDetailAsync(route);

                return new ExternalEntryProcessResult
                {
                    Succeeded = true,
                    Outcome = ExternalEntryOutcome.Opened,
                    Route = route,
                    UserMessage = "Opening saved location..."
                };
            }

            var failedResult = new ExternalEntryProcessResult
            {
                Outcome = ExternalEntryOutcome.OpenFailed,
                Route = route,
                UserMessage = "We couldn't open that location."
            };
            await _statusService.ShowErrorAsync(failedResult.UserMessage);
            return failedResult;
        }
    }

    private ExternalEntryProcessResult CreateInvalidResult(ExternalStallRoute route)
    {
        var isUnsupportedRoute = route.RawPayload.Contains("://", StringComparison.Ordinal) &&
                                 route.ErrorMessage.Contains("not supported", StringComparison.OrdinalIgnoreCase);

        return new ExternalEntryProcessResult
        {
            Outcome = isUnsupportedRoute
                ? ExternalEntryOutcome.UnsupportedRoute
                : ExternalEntryOutcome.InvalidPayload,
            Route = route,
            UserMessage = isUnsupportedRoute
                ? "That link is not supported."
                : "That code could not be opened."
        };
    }

    private bool ShouldIgnoreDuplicate(ExternalStallRoute route)
    {
        var key = BuildNavigationKey(route);
        var now = _timeProvider.GetUtcNow();

        lock (_syncRoot)
        {
            return string.Equals(_lastNavigationKey, key, StringComparison.Ordinal) &&
                   now - _lastNavigationAt <= DuplicateCooldown;
        }
    }

    private void RememberNavigation(ExternalStallRoute route)
    {
        var key = BuildNavigationKey(route);
        var now = _timeProvider.GetUtcNow();

        lock (_syncRoot)
        {
            _lastNavigationKey = key;
            _lastNavigationAt = now;
        }
    }

    private static string BuildNavigationKey(ExternalStallRoute route)
    {
        return $"{route.SourceType}:{route.RawPayload.Trim().ToLowerInvariant()}";
    }
}
