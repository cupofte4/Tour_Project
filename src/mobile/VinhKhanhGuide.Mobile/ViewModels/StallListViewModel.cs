using System.Collections.ObjectModel;
using System.Globalization;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.ViewModels;

public class StallListViewModel : ViewModelBase
{
    private static readonly IReadOnlyList<LanguageOption> PreviewLanguages =
    [
        new LanguageOption { Code = "vi", DisplayName = "Vietnamese" },
        new LanguageOption { Code = "en", DisplayName = "English" },
        new LanguageOption { Code = "zh", DisplayName = "Chinese" },
        new LanguageOption { Code = "de", DisplayName = "German" }
    ];

    private readonly IStallDataService _stallDataService;
    private readonly INarrationService _narrationService;
    private readonly ISettingsService _settingsService;
    private bool _hasLoaded;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private string _noticeMessage = string.Empty;
    private StallSummary? _selectedStall;
    private LanguageOption? _selectedPreviewLanguage;
    private string _previewStatusText = "Choose a language to listen to a short narration preview.";

    public static StallListViewModel CreateForApiClient(
        IStallApiClient stallApiClient,
        INarrationService narrationService,
        ISettingsService settingsService)
    {
        return new StallListViewModel(
            new DirectStallDataService(stallApiClient),
            narrationService,
            settingsService);
    }

    public StallListViewModel(
        IStallDataService stallDataService,
        INarrationService narrationService,
        ISettingsService settingsService)
    {
        _stallDataService = stallDataService;
        _narrationService = narrationService;
        _settingsService = settingsService;
        _narrationService.PlaybackStateChanged += OnNarrationPlaybackStateChanged;

        var preferredLanguageCode = _settingsService.GetSettings().PreferredTtsLanguage;
        var defaultLanguageCode = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(
            string.IsNullOrWhiteSpace(preferredLanguageCode)
                ? CultureInfo.CurrentUICulture.Name
                : preferredLanguageCode);
        SelectedPreviewLanguage = PreviewLanguages.FirstOrDefault(language =>
            string.Equals(language.Code, defaultLanguageCode, StringComparison.OrdinalIgnoreCase))
            ?? PreviewLanguages.First(language => string.Equals(language.Code, "en", StringComparison.OrdinalIgnoreCase));
    }

    public ObservableCollection<StallSummary> Stalls { get; } = [];

    public IReadOnlyList<LanguageOption> SupportedPreviewLanguages => PreviewLanguages;

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

    public string NoticeMessage
    {
        get => _noticeMessage;
        private set
        {
            if (SetProperty(ref _noticeMessage, value))
            {
                OnPropertyChanged(nameof(HasNoticeMessage));
            }
        }
    }

    public bool HasNoticeMessage => !string.IsNullOrWhiteSpace(NoticeMessage);

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

    public LanguageOption? SelectedPreviewLanguage
    {
        get => _selectedPreviewLanguage;
        set => SetProperty(ref _selectedPreviewLanguage, value);
    }

    public string PreviewStatusText
    {
        get => _previewStatusText;
        private set => SetProperty(ref _previewStatusText, value);
    }

    public string LoadingMessage => "Loading locations...";

    public string EmptyStateTitle => HasError
        ? "Locations are unavailable"
        : "No locations yet";

    public string EmptyStateMessage => HasError
        ? "Please try again in a moment."
        : "Locations will appear here when they are ready.";

    public async Task LoadAsync()
    {
        if (_hasLoaded || IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        NoticeMessage = string.Empty;

        try
        {
            var cachedStalls = await _stallDataService.GetCachedStallsAsync();
            if (cachedStalls.Count > 0)
            {
                ApplyStalls(cachedStalls);
                NoticeMessage = "Showing saved locations while refreshing.";
            }

            var stalls = await _stallDataService.RefreshStallsAsync();
            ApplyStalls(stalls);
            NoticeMessage = string.Empty;

            _hasLoaded = true;
        }
        catch (Exception)
        {
            var cachedStalls = await _stallDataService.GetCachedStallsAsync();
            if (cachedStalls.Count > 0)
            {
                ApplyStalls(cachedStalls);
                NoticeMessage = "Showing saved locations because the latest content couldn't be refreshed.";
                _hasLoaded = true;
                return;
            }

            ErrorMessage = "We couldn't load locations right now.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task PreviewNarrationAsync(StallSummary? stall)
    {
        if (stall is null)
        {
            PreviewStatusText = "Narration preview is not available right now.";
            return;
        }

        var requestedLanguage = SelectedPreviewLanguage?.Code ?? "en";
        var resolvedNarration = await StallNarrationResolver.ResolveAsync(
            requestedLanguage,
            stall.Name,
            StallNarrationResolver.ResolveVietnameseNarrationText(stall.NarrationScriptVi, stall.DescriptionVi),
            ResolvePreferredAudioSource(stall),
            languageCode => Task.FromResult(stall.Translations.FirstOrDefault(item =>
                string.Equals(item.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase))));

        if (!resolvedNarration.CanNarrate)
        {
            PreviewStatusText = $"Narration preview is not available for {stall.Name}.";
            return;
        }

        PreviewStatusText = $"Starting narration preview for {stall.Name}.";

        await _narrationService.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = stall.Id,
            Priority = stall.Priority,
            Title = resolvedNarration.Name,
            AudioUrl = resolvedNarration.AudioUrl,
            Text = resolvedNarration.Text,
            RequestedLanguageCode = resolvedNarration.RequestedLanguageCode,
            LanguageCode = resolvedNarration.LanguageCode,
            LocaleCode = resolvedNarration.LocaleCode,
            UsedFallback = resolvedNarration.UsedFallback
        });
    }

    private async Task OpenDetailAsync(int stallId)
    {
        SelectedStall = null;
        await Shell.Current.GoToAsync($"stall-detail?stallId={stallId}");
    }

    private void OnNarrationPlaybackStateChanged(NarrationPlaybackState state)
    {
        PreviewStatusText = state.Status switch
        {
            NarrationPlaybackStatus.Idle => "Choose a language to listen to a short narration preview.",
            NarrationPlaybackStatus.Queued => "Starting narration preview...",
            NarrationPlaybackStatus.Preparing => "Preparing narration...",
            NarrationPlaybackStatus.Playing => "Narration is playing.",
            NarrationPlaybackStatus.Stopped => string.IsNullOrWhiteSpace(state.Message)
                ? "Narration stopped."
                : "Narration stopped.",
            NarrationPlaybackStatus.Error => string.IsNullOrWhiteSpace(state.Message)
                ? "We couldn't start narration."
                : state.Message,
            _ => "Choose a language to listen to a short narration preview."
        };
    }

    private void ApplyStalls(IReadOnlyList<StallSummary> stalls)
    {
        Stalls.Clear();

        foreach (var stall in stalls)
        {
            Stalls.Add(stall);
        }
    }

    private static string ResolvePreferredAudioSource(StallSummary stall)
    {
        return NarrationAudioSourceResolver.ResolvePreferredAudioSource(stall.LocalAudioPath, stall.AudioUrl);
    }
}
