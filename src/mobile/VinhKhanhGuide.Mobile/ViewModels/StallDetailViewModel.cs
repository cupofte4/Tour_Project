using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.ViewModels;

public class StallDetailViewModel(IStallApiClient stallApiClient) : ViewModelBase
{
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private StallDetail? _stall;
    private int _loadedStallId;

    public StallDetail? Stall
    {
        get => _stall;
        private set => SetProperty(ref _stall, value);
    }

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

    public async Task LoadAsync(int stallId)
    {
        if (stallId <= 0 || (_loadedStallId == stallId && Stall is not null) || IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            Stall = await stallApiClient.GetStallByIdAsync(stallId);
            _loadedStallId = stallId;

            if (Stall is null)
            {
                ErrorMessage = "Stall not found.";
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load stall details from the backend.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
