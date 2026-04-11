using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile;

public partial class MainPage : ContentPage
{
    private readonly StallListViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = ServiceHelper.GetRequiredService<StallListViewModel>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }

    private async void OnPreviewNarrationClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: StallSummary stall })
        {
            await _viewModel.PreviewNarrationAsync(stall);
        }
    }
}
