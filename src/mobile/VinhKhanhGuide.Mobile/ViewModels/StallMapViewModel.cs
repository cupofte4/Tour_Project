using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.ViewModels;

public class StallMapViewModel(
    IStallApiClient stallApiClient,
    ILocationService locationService,
    IProximityService proximityService) : ViewModelBase
{
    private bool _hasLoaded;
    private bool _isLoading;
    private string _statusMessage = string.Empty;
    private Location? _mapCenter;
    private NearbyStallNotification? _nearbyStall;

    public ObservableCollection<StallSummary> Stalls { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public Location? MapCenter
    {
        get => _mapCenter;
        private set => SetProperty(ref _mapCenter, value);
    }

    public NearbyStallNotification? NearbyStall
    {
        get => _nearbyStall;
        private set
        {
            if (SetProperty(ref _nearbyStall, value))
            {
                OnPropertyChanged(nameof(HasNearbyStall));
            }
        }
    }

    public bool HasNearbyStall => NearbyStall is not null;

    public async Task LoadAsync()
    {
        if (_hasLoaded || IsLoading)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            var stalls = await stallApiClient.GetStallsAsync();

            Stalls.Clear();

            foreach (var stall in stalls)
            {
                Stalls.Add(stall);
            }

            await RefreshLocationAsync();
            _hasLoaded = true;
        }
        catch (Exception)
        {
            StatusMessage = "Could not load stall map data from the backend.";

            if (Stalls.Count > 0)
            {
                MapCenter = new Location(Stalls[0].Latitude, Stalls[0].Longitude);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshLocationAsync()
    {
        var locationResult = await locationService.GetCurrentLocationAsync();

        if (locationResult.CurrentLocation is not null)
        {
            MapCenter = new Location(
                locationResult.CurrentLocation.Value.Latitude,
                locationResult.CurrentLocation.Value.Longitude);

            var nearbyStall = proximityService.EvaluateNearbyStall(
                locationResult.CurrentLocation.Value,
                Stalls,
                DateTimeOffset.UtcNow);

            if (nearbyStall is not null)
            {
                NearbyStall = nearbyStall;
            }
        }
        else if (Stalls.Count > 0 && MapCenter is null)
        {
            MapCenter = new Location(Stalls[0].Latitude, Stalls[0].Longitude);
        }

        StatusMessage = locationResult.Message;
    }

    public void DismissNearbyStall()
    {
        NearbyStall = null;
    }

    public Task OpenNearbyStallAsync()
    {
        if (NearbyStall is null)
        {
            return Task.CompletedTask;
        }

        var stallId = NearbyStall.StallId;
        NearbyStall = null;

        return OpenStallDetailAsync(stallId);
    }

    public Task OpenStallDetailAsync(int stallId)
    {
        return Shell.Current.GoToAsync($"stall-detail?stallId={stallId}");
    }
}
