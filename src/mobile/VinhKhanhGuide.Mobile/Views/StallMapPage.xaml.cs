using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.ComponentModel;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile.Views;

public partial class StallMapPage : ContentPage
{
    private readonly StallMapViewModel _viewModel;
    private bool _hasAppliedInitialMapRegion;
    private bool _hasCenteredOnUserLocation;

    public StallMapPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = ServiceHelper.GetRequiredService<StallMapViewModel>();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
        await _viewModel.StartLocationTrackingAsync();
        RefreshMap();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _viewModel.StopLocationTrackingAsync();
    }

    private void RefreshMap()
    {
        StallsMap.Pins.Clear();
        var nearestStallId = _viewModel.NearestStall?.Id;

        foreach (var stall in _viewModel.Stalls)
        {
            var isNearestStall = nearestStallId == stall.Id;
            var pin = new Pin
            {
                Label = isNearestStall ? $"Nearest: {stall.Name}" : stall.Name,
                Address = stall.Category,
                Type = isNearestStall ? PinType.SavedPin : PinType.Place,
                Location = new Location(stall.Latitude, stall.Longitude)
            };

            pin.MarkerClicked += async (_, args) =>
            {
                args.HideInfoWindow = false;
                await _viewModel.OpenStallDetailAsync(stall.Id);
            };

            StallsMap.Pins.Add(pin);
        }

        if (_viewModel.UserLocation is not null)
        {
            StallsMap.Pins.Add(new Pin
            {
                Label = "You are here",
                Address = "Current location",
                Type = PinType.Generic,
                Location = _viewModel.UserLocation
            });
        }

        ApplyMapRegionIfNeeded();
    }

    private void ApplyMapRegionIfNeeded()
    {
        if (!_hasAppliedInitialMapRegion)
        {
            if (_viewModel.UserLocation is not null)
            {
                StallsMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                    _viewModel.UserLocation,
                    Distance.FromMeters(400)));
                _hasAppliedInitialMapRegion = true;
                _hasCenteredOnUserLocation = true;
                return;
            }

            var initialCenter = _viewModel.MapCenter;

            if (initialCenter is not null)
            {
                StallsMap.MoveToRegion(MapSpan.FromCenterAndRadius(initialCenter, Distance.FromMeters(500)));
                _hasAppliedInitialMapRegion = true;
                return;
            }

            if (_viewModel.Stalls.Count > 0)
            {
                var firstStall = _viewModel.Stalls[0];
                StallsMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(firstStall.Latitude, firstStall.Longitude),
                    Distance.FromMeters(500)));
                _hasAppliedInitialMapRegion = true;
                return;
            }
        }

        if (StallMapStateHelper.ShouldCenterOnUserLocation(_hasCenteredOnUserLocation, _viewModel.UserLocation))
        {
            StallsMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                _viewModel.UserLocation!,
                Distance.FromMeters(400)));
            _hasCenteredOnUserLocation = true;
            _hasAppliedInitialMapRegion = true;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(StallMapViewModel.MapCenter)
            or nameof(StallMapViewModel.StatusMessage)
            or nameof(StallMapViewModel.UserLocation)
            or nameof(StallMapViewModel.NearestStall))
        {
            RefreshMap();
        }
    }

    private async void OnOpenNearestStallClicked(object? sender, EventArgs e)
    {
        await _viewModel.OpenNearestStallAsync();
    }

    private async void OnOpenNearbyStallClicked(object? sender, EventArgs e)
    {
        await _viewModel.OpenNearbyStallAsync();
    }

    private async void OnPlayNearbyNarrationClicked(object? sender, EventArgs e)
    {
        await _viewModel.StartNearbyStallNarrationAsync();
    }

    private void OnDismissNearbyStallClicked(object? sender, EventArgs e)
    {
        _viewModel.DismissNearbyStall();
    }
}
