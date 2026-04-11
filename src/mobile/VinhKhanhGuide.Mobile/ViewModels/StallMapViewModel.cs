using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.ViewModels;

public class StallMapViewModel : ViewModelBase
{
    private readonly IStallApiClient _stallApiClient;
    private readonly IProximityDistanceCalculator _distanceCalculator;
    private readonly ILocationTrackingService _locationTrackingService;
    private readonly IProximityService _proximityService;
    private readonly IProximityNarrationCoordinator _proximityNarrationCoordinator;
    private readonly SemaphoreSlim _locationUpdateLock = new(1, 1);
    private bool _hasLoaded;
    private bool _isLoading;
    private bool _isSubscribedToLocationUpdates;
    private string _statusMessage = string.Empty;
    private Location? _mapCenter;
    private Location? _userLocation;
    private NearbyStallNotification? _nearbyStall;
    private StallSummary? _nearestStall;
    private string _nearestStallDistanceText = string.Empty;

    public StallMapViewModel(
        IStallApiClient stallApiClient,
        IProximityDistanceCalculator distanceCalculator,
        ILocationTrackingService locationTrackingService,
        IProximityService proximityService,
        IProximityNarrationCoordinator proximityNarrationCoordinator)
    {
        _stallApiClient = stallApiClient;
        _distanceCalculator = distanceCalculator;
        _locationTrackingService = locationTrackingService;
        _proximityService = proximityService;
        _proximityNarrationCoordinator = proximityNarrationCoordinator;
    }

    public ObservableCollection<StallSummary> Stalls { get; } = [];

    public bool IsTracking => _locationTrackingService.IsTracking;

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

    public Location? UserLocation
    {
        get => _userLocation;
        private set => SetProperty(ref _userLocation, value);
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

    public StallSummary? NearestStall
    {
        get => _nearestStall;
        private set
        {
            if (SetProperty(ref _nearestStall, value))
            {
                OnPropertyChanged(nameof(HasNearestStall));
            }
        }
    }

    public bool HasNearestStall => NearestStall is not null;

    public string NearestStallDistanceText
    {
        get => _nearestStallDistanceText;
        private set => SetProperty(ref _nearestStallDistanceText, value);
    }

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
            var stalls = await _stallApiClient.GetStallsAsync();

            Stalls.Clear();

            foreach (var stall in stalls)
            {
                Stalls.Add(stall);
            }

            _proximityService.Reset();
            NearbyStall = _proximityNarrationCoordinator.DismissPrompt();
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
        var locationResult = await _locationTrackingService.GetCurrentLocationAsync();
        await ApplyLocationResultAsync(locationResult);
    }

    public async Task StartLocationTrackingAsync()
    {
        SubscribeToLocationUpdates();
        var locationResult = await _locationTrackingService.StartTrackingAsync();
        await ApplyLocationResultAsync(locationResult);
        OnPropertyChanged(nameof(IsTracking));
    }

    public async Task StopLocationTrackingAsync()
    {
        await _locationTrackingService.StopTrackingAsync();
        UnsubscribeFromLocationUpdates();
        OnPropertyChanged(nameof(IsTracking));
    }

    private async Task ApplyLocationResultAsync(LocationResult locationResult)
    {
        await _locationUpdateLock.WaitAsync();

        try
        {
            if (locationResult.CurrentLocation is not null)
            {
                UserLocation = new Location(
                    locationResult.CurrentLocation.Value.Latitude,
                    locationResult.CurrentLocation.Value.Longitude);

                MapCenter ??= UserLocation;

                var nearbyStall = _proximityService.ProcessLocationUpdate(
                    locationResult.CurrentLocation.Value,
                    Stalls,
                    DateTimeOffset.UtcNow);

                if (nearbyStall is not null)
                {
                    NearbyStall = await _proximityNarrationCoordinator.HandleTriggerAsync(nearbyStall, Stalls);
                }

                var nearestStall = StallMapStateHelper.FindNearestActiveStall(
                    locationResult.CurrentLocation.Value,
                    Stalls,
                    _distanceCalculator);

                NearestStall = nearestStall?.Stall;
                NearestStallDistanceText = nearestStall is null
                    ? string.Empty
                    : $"{Math.Round(nearestStall.DistanceMeters)} m away";
            }
            else
            {
                UserLocation = null;

                if (Stalls.Count > 0 && MapCenter is null)
                {
                    MapCenter = new Location(Stalls[0].Latitude, Stalls[0].Longitude);
                }
            }

            StatusMessage = locationResult.Message;
        }
        finally
        {
            _locationUpdateLock.Release();
        }
    }

    private void OnLocationUpdated(LocationResult locationResult)
    {
        _ = ApplyLocationResultAsync(locationResult);
    }

    private void SubscribeToLocationUpdates()
    {
        if (_isSubscribedToLocationUpdates)
        {
            return;
        }

        _locationTrackingService.LocationUpdated += OnLocationUpdated;
        _isSubscribedToLocationUpdates = true;
    }

    private void UnsubscribeFromLocationUpdates()
    {
        if (!_isSubscribedToLocationUpdates)
        {
            return;
        }

        _locationTrackingService.LocationUpdated -= OnLocationUpdated;
        _isSubscribedToLocationUpdates = false;
    }

    public void DismissNearbyStall()
    {
        NearbyStall = _proximityNarrationCoordinator.DismissPrompt();
    }

    public Task OpenNearestStallAsync()
    {
        if (NearestStall is null)
        {
            return Task.CompletedTask;
        }

        return OpenStallDetailAsync(NearestStall.Id);
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

    public async Task StartNearbyStallNarrationAsync()
    {
        NearbyStall = await _proximityNarrationCoordinator.StartPromptNarrationAsync(Stalls);
    }

    public Task OpenStallDetailAsync(int stallId)
    {
        return Shell.Current.GoToAsync($"stall-detail?stallId={stallId}");
    }
}
