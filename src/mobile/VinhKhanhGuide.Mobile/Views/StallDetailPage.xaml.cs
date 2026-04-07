using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Views;

[QueryProperty(nameof(StallId), "stallId")]
public partial class StallDetailPage : ContentPage
{
    private readonly StallDetailViewModel _viewModel;
    private int _stallId;

    public StallDetailPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = ServiceHelper.GetRequiredService<StallDetailViewModel>();
    }

    public string StallId
    {
        get => _stallId.ToString();
        set
        {
            if (int.TryParse(value, out var stallId))
            {
                _stallId = stallId;
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync(_stallId);
    }
}
