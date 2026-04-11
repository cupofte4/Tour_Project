using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Views;

[QueryProperty(nameof(StallId), "stallId")]
public partial class StallDetailPage : ContentPage
{
    private readonly StallDetailViewModel _viewModel;
    private string _stallId = string.Empty;

    public StallDetailPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = ServiceHelper.GetRequiredService<StallDetailViewModel>();
    }

    public string StallId
    {
        get => _stallId;
        set
        {
            _stallId = value;

            if (int.TryParse(value, out var stallId))
            {
                _ = _viewModel.LoadAsync(stallId);
            }
        }
    }

    private async void OnPlayNarrationClicked(object? sender, EventArgs e)
    {
        await _viewModel.StartNarrationAsync();
    }

    private async void OnStopNarrationClicked(object? sender, EventArgs e)
    {
        await _viewModel.StopNarrationAsync();
    }
}
