using Microsoft.Maui.Graphics;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class ExternalEntryStatusService : ViewModelBase
{
    private string _message = string.Empty;
    private bool _isVisible;
    private Color _backgroundColor = Colors.Transparent;
    private Color _textColor = Colors.Black;
    private CancellationTokenSource? _dismissCts;

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        private set => SetProperty(ref _isVisible, value);
    }

    public Color BackgroundColor
    {
        get => _backgroundColor;
        private set => SetProperty(ref _backgroundColor, value);
    }

    public Color TextColor
    {
        get => _textColor;
        private set => SetProperty(ref _textColor, value);
    }

    public Task ShowInfoAsync(string message, TimeSpan? duration = null)
    {
        return ShowAsync(message, Color.FromArgb("#F4F1E8"), Color.FromArgb("#5C4B2C"), duration ?? TimeSpan.FromSeconds(1.8));
    }

    public Task ShowErrorAsync(string message, TimeSpan? duration = null)
    {
        return ShowAsync(message, Color.FromArgb("#F8E8E6"), Color.FromArgb("#7A2E24"), duration ?? TimeSpan.FromSeconds(2.4));
    }

    public Task ShowAsync(string message, Color backgroundColor, Color textColor, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Clear();
            return Task.CompletedTask;
        }

        var dismissCts = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _dismissCts, dismissCts);
        previous?.Cancel();
        previous?.Dispose();

        Message = message;
        BackgroundColor = backgroundColor;
        TextColor = textColor;
        IsVisible = true;

        _ = DismissLaterAsync(dismissCts, duration);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        var previous = Interlocked.Exchange(ref _dismissCts, null);
        previous?.Cancel();
        previous?.Dispose();

        Message = string.Empty;
        IsVisible = false;
    }

    private async Task DismissLaterAsync(CancellationTokenSource dismissCts, TimeSpan duration)
    {
        try
        {
            await Task.Delay(duration, dismissCts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (ReferenceEquals(_dismissCts, dismissCts))
        {
            Clear();
        }
    }
}
