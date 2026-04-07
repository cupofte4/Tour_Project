using System.Collections.ObjectModel;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.ViewModels;

public class StallListViewModel(IStallApiClient stallApiClient) : ViewModelBase
{
    private bool _hasLoaded;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private StallSummary? _selectedStall;

    public ObservableCollection<StallSummary> Stalls { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public StallSummary? SelectedStall
    {
        get => _selectedStall;
        set
        {
            if (SetProperty(ref _selectedStall, value) && value is not null)
            {
                _ = OpenDetailAsync(value.Id);
            }
        }
    }

    public async Task LoadAsync()
    {
        if (_hasLoaded || IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var stalls = await stallApiClient.GetStallsAsync();

            Stalls.Clear();

            foreach (var stall in stalls)
            {
                Stalls.Add(stall);
            }

            _hasLoaded = true;
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load stalls from the backend.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OpenDetailAsync(int stallId)
    {
        SelectedStall = null;
        await Shell.Current.GoToAsync($"stall-detail?stallId={stallId}");
    }
}
