using System.Globalization;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.ViewModels;

public class StallDetailViewModel : ViewModelBase
{
    private static readonly IReadOnlyList<LanguageOption> SupportedLanguages =
    [
        new LanguageOption { Code = "vi", DisplayName = "Vietnamese" },
        new LanguageOption { Code = "en", DisplayName = "English" },
        new LanguageOption { Code = "zh", DisplayName = "Chinese" },
        new LanguageOption { Code = "de", DisplayName = "German" }
    ];

    private readonly IStallApiClient _stallApiClient;
    private readonly INarrationService _narrationService;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private StallDetail? _stall;
    private int _loadedStallId;
    private LanguageOption? _selectedLanguage;
    private string _displayName = string.Empty;
    private string _displayDescription = string.Empty;
    private NarrationPlaybackState _narrationState = NarrationPlaybackState.Idle;
    private ResolvedStallNarration? _resolvedNarration;

    public StallDetailViewModel(IStallApiClient stallApiClient, INarrationService narrationService)
    {
        _stallApiClient = stallApiClient;
        _narrationService = narrationService;
        _narrationService.PlaybackStateChanged += OnNarrationPlaybackStateChanged;
    }

    public IReadOnlyList<LanguageOption> Languages => SupportedLanguages;

    public StallDetail? Stall
    {
        get => _stall;
        private set
        {
            if (SetProperty(ref _stall, value) && value is not null)
            {
                DisplayName = value.Name;
                DisplayDescription = StallNarrationResolver.ResolveVietnameseNarrationText(
                    value.NarrationScriptVi,
                    value.DescriptionVi);
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(CanStartNarration));
            }
        }
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

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public string DisplayDescription
    {
        get => _displayDescription;
        private set => SetProperty(ref _displayDescription, value);
    }

    public LanguageOption? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value) && value is not null && _loadedStallId > 0)
            {
                _ = LoadTranslationAsync(_loadedStallId, value.Code);
            }
        }
    }

    public NarrationPlaybackState NarrationState
    {
        get => _narrationState;
        private set
        {
            if (SetProperty(ref _narrationState, value))
            {
                OnPropertyChanged(nameof(IsNarrating));
                OnPropertyChanged(nameof(NarrationStatusText));
                OnPropertyChanged(nameof(HasNarrationStatus));
                OnPropertyChanged(nameof(IsNarrationAvailable));
                OnPropertyChanged(nameof(CanStartNarration));
                OnPropertyChanged(nameof(CanStopNarration));
            }
        }
    }

    public bool IsNarrating => NarrationState.Status is NarrationPlaybackStatus.Queued
        or NarrationPlaybackStatus.Preparing
        or NarrationPlaybackStatus.Playing;

    public string NarrationStatusText => NarrationState.Status switch
    {
        NarrationPlaybackStatus.Idle when IsNarrationAvailable => "Ready to play narration.",
        NarrationPlaybackStatus.Idle => "Narration unavailable for this stall.",
        NarrationPlaybackStatus.Queued => "Narration queued...",
        NarrationPlaybackStatus.Preparing => "Preparing narration...",
        NarrationPlaybackStatus.Playing => "Narration playing...",
        NarrationPlaybackStatus.Error => NarrationState.Message,
        NarrationPlaybackStatus.Stopped => string.IsNullOrWhiteSpace(NarrationState.Message)
            ? "Narration stopped."
            : NarrationState.Message,
        _ => string.Empty
    };

    public bool HasNarrationStatus => true;

    public bool IsNarrationAvailable => _resolvedNarration?.CanNarrate == true;

    public bool CanStartNarration => Stall is not null &&
                                     IsNarrationAvailable &&
                                     !IsLoading &&
                                     (!IsNarrating || NarrationState.ActivePoiId != Stall.Id);

    public bool CanStopNarration => IsNarrating && Stall is not null && NarrationState.ActivePoiId == Stall.Id;

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
            Stall = await _stallApiClient.GetStallByIdAsync(stallId);
            _loadedStallId = stallId;

            if (Stall is null)
            {
                ErrorMessage = "Stall not found.";
            }
            else if (SelectedLanguage is null)
            {
                var defaultLanguageCode = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(CultureInfo.CurrentUICulture.Name);
                SelectedLanguage = Languages.First(language =>
                    string.Equals(language.Code, defaultLanguageCode, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                await LoadTranslationAsync(stallId, SelectedLanguage.Code);
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

    private async Task LoadTranslationAsync(int stallId, string languageCode)
    {
        if (Stall is null)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var resolvedContent = await ResolveLocalizedContentAsync(stallId, languageCode);
            DisplayName = resolvedContent.Name;
            DisplayDescription = resolvedContent.Text;
            _resolvedNarration = resolvedContent;
            OnPropertyChanged(nameof(IsNarrationAvailable));
            OnPropertyChanged(nameof(CanStartNarration));
            OnPropertyChanged(nameof(NarrationStatusText));
        }
        catch (Exception)
        {
            DisplayName = Stall.Name;
            DisplayDescription = StallNarrationResolver.ResolveVietnameseNarrationText(
                Stall.NarrationScriptVi,
                Stall.DescriptionVi);
            _resolvedNarration = await StallNarrationResolver.ResolveAsync(
                "vi",
                Stall.Name,
                DisplayDescription,
                Stall.AudioUrl,
                languageCode => Task.FromResult<StallTranslation?>(null));
            ErrorMessage = "Could not load translated content. Showing default narration text.";
            OnPropertyChanged(nameof(IsNarrationAvailable));
            OnPropertyChanged(nameof(CanStartNarration));
            OnPropertyChanged(nameof(NarrationStatusText));
        }
        finally
        {
            IsLoading = false;
        }
    }

    public Task StartNarrationAsync()
    {
        if (Stall is null)
        {
            return Task.CompletedTask;
        }

        return _narrationService.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = Stall.Id,
            Priority = Stall.Priority,
            Title = DisplayName,
            AudioUrl = _resolvedNarration?.AudioUrl ?? string.Empty,
            Text = _resolvedNarration?.Text ?? ResolveNarrationText(),
            LanguageCode = _resolvedNarration?.LanguageCode ?? SelectedLanguage?.Code ?? "en"
        });
    }

    public Task StopNarrationAsync()
    {
        return _narrationService.StopAsync();
    }

    private string ResolveNarrationText()
    {
        if (!string.IsNullOrWhiteSpace(DisplayDescription))
        {
            return DisplayDescription;
        }

        return Stall is null
            ? string.Empty
            : StallNarrationResolver.ResolveVietnameseNarrationText(
                Stall.NarrationScriptVi,
                Stall.DescriptionVi);
    }

    private void OnNarrationPlaybackStateChanged(NarrationPlaybackState state)
    {
        NarrationState = state;
        OnPropertyChanged(nameof(HasNarrationStatus));
    }

    private async Task<ResolvedStallNarration> ResolveLocalizedContentAsync(int stallId, string requestedLanguageCode)
    {
        if (Stall is null)
        {
            return await StallNarrationResolver.ResolveAsync(
                requestedLanguageCode,
                string.Empty,
                string.Empty,
                string.Empty,
                languageCode => Task.FromResult<StallTranslation?>(null));
        }

        return await StallNarrationResolver.ResolveAsync(
            requestedLanguageCode,
            Stall.Name,
            StallNarrationResolver.ResolveVietnameseNarrationText(
                Stall.NarrationScriptVi,
                Stall.DescriptionVi),
            Stall.AudioUrl,
            async candidateLanguage =>
            {
            var translation = Stall.Translations.FirstOrDefault(item =>
                string.Equals(item.LanguageCode, candidateLanguage, StringComparison.OrdinalIgnoreCase));

            translation ??= await _stallApiClient.GetStallTranslationAsync(stallId, candidateLanguage);
            return translation;
        });
    }
}
