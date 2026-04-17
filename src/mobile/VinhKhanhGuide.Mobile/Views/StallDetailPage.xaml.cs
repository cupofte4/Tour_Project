using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Views;

public partial class StallDetailPage : ContentPage, IQueryAttributable
{
    private readonly StallDetailViewModel _viewModel;
    private readonly ExternalEntryStatusService _externalEntryStatusService;
    private StallDetailOpenRequest? _pendingOpenRequest;
    private int _openRequestVersion;
    private bool _hasAppeared;

    public StallDetailPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = ServiceHelper.GetRequiredService<StallDetailViewModel>();
        _externalEntryStatusService = ServiceHelper.GetRequiredService<ExternalEntryStatusService>();
        ExternalEntryBanner.BindingContext = _externalEntryStatusService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!TryGetInt(query, "stallId", out var stallId) || stallId <= 0)
        {
            return;
        }

        _pendingOpenRequest = new StallDetailOpenRequest
        {
            StallId = stallId,
            AutoPlay = TryGetString(query, "autoplay") == "1",
            RequestedLanguageCode = TryGetString(query, "lang")
        };

        if (_hasAppeared)
        {
            _ = ProcessPendingOpenRequestAsync();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _hasAppeared = true;
        _ = ProcessPendingOpenRequestAsync();
    }

    private async Task ProcessPendingOpenRequestAsync()
    {
        if (_pendingOpenRequest is null)
        {
            return;
        }

        var request = _pendingOpenRequest;
        _pendingOpenRequest = null;
        var requestVersion = Interlocked.Increment(ref _openRequestVersion);

        await _viewModel.OpenAsync(
            request.StallId,
            request.RequestedLanguageCode,
            request.AutoPlay,
            requestVersion);
    }

    private static bool TryGetInt(IDictionary<string, object> query, string key, out int value)
    {
        if (int.TryParse(TryGetString(query, key), out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    private static string? TryGetString(IDictionary<string, object> query, string key)
    {
        if (!query.TryGetValue(key, out var value))
        {
            return null;
        }

        return value switch
        {
            string stringValue => stringValue,
            _ => value?.ToString()
        };
    }

    private async void OnPlayNarrationClicked(object? sender, EventArgs e)
    {
        await _viewModel.StartNarrationAsync();
    }

    private async void OnStopNarrationClicked(object? sender, EventArgs e)
    {
        await _viewModel.StopNarrationAsync();
    }

    private async void OnDownloadOfflineAudioClicked(object? sender, EventArgs e)
    {
        await _viewModel.DownloadOfflineAudioAsync();
    }

    private async void OnRemoveOfflineAudioClicked(object? sender, EventArgs e)
    {
        await _viewModel.RemoveOfflineAudioAsync();
    }
}
