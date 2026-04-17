using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile;

public partial class App : Application
{
    private readonly IExternalEntryService _externalEntryService;
    private readonly IUsageAnalyticsService _usageAnalyticsService;

    public App(
        AppShell appShell,
        IExternalEntryService externalEntryService,
        IUsageAnalyticsService usageAnalyticsService)
    {
        _externalEntryService = externalEntryService;
        _usageAnalyticsService = usageAnalyticsService;
        InitializeComponent();
        MainPage = appShell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Activated += OnWindowActivated;
        window.Stopped += OnWindowStopped;
        window.Destroying += OnWindowDestroying;
        return window;
    }

    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);
        _ = HandleExternalPayloadAsync(uri.AbsoluteUri, ExternalEntrySourceType.DeepLink);
    }

    public async Task<ExternalEntryProcessResult> HandleExternalPayloadAsync(
        string? rawPayload,
        ExternalEntrySourceType sourceType,
        CancellationToken cancellationToken = default)
    {
        return await _externalEntryService.ProcessAsync(rawPayload, sourceType, cancellationToken);
    }

    private void OnWindowActivated(object? sender, EventArgs e)
    {
        _ = _usageAnalyticsService.OnAppBecameActiveAsync();
    }

    private void OnWindowStopped(object? sender, EventArgs e)
    {
        _ = _usageAnalyticsService.OnAppWentToBackgroundAsync();
    }

    private void OnWindowDestroying(object? sender, EventArgs e)
    {
        _ = _usageAnalyticsService.OnAppWentToBackgroundAsync();
    }
}
