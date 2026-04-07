using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Views;

public partial class StallMapPage : ContentPage
{
    private readonly StallMapViewModel _viewModel;
    private IDispatcherTimer? _locationTimer;

    public StallMapPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = ServiceHelper.GetRequiredService<StallMapViewModel>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
        RefreshMap();
        StartLocationPolling();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _locationTimer?.Stop();
    }

    private void RefreshMap()
    {
        StallsMap.Pins.Clear();

        foreach (var stall in _viewModel.Stalls)
        {
            var pin = new Pin
            {
                Label = stall.Name,
                Address = stall.Category,
                Type = PinType.Place,
                Location = new Location(stall.Latitude, stall.Longitude)
            };

            pin.MarkerClicked += async (_, args) =>
            {
                args.HideInfoWindow = false;
                await _viewModel.OpenStallDetailAsync(stall.Id);
            };

            StallsMap.Pins.Add(pin);
        }

        var center = _viewModel.MapCenter;

        if (center is not null)
        {
            StallsMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromMeters(500)));
        }
        else if (_viewModel.Stalls.Count > 0)
        {
            var firstStall = _viewModel.Stalls[0];
            StallsMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(firstStall.Latitude, firstStall.Longitude),
                Distance.FromMeters(500)));
        }
    }

    private void StartLocationPolling()
    {
        _locationTimer ??= Dispatcher.CreateTimer();
        _locationTimer.Interval = TimeSpan.FromSeconds(5);
        _locationTimer.Tick -= OnLocationTimerTick;
        _locationTimer.Tick += OnLocationTimerTick;

        if (!_locationTimer.IsRunning)
        {
            _locationTimer.Start();
        }
    }

    private async void OnLocationTimerTick(object? sender, EventArgs e)
    {
        await _viewModel.RefreshLocationAsync();
        RefreshMap();
    }

    private async void OnOpenNearbyStallClicked(object? sender, EventArgs e)
    {
        await _viewModel.OpenNearbyStallAsync();
    }

    private void OnDismissNearbyStallClicked(object? sender, EventArgs e)
    {
        _viewModel.DismissNearbyStall();
    }
}
