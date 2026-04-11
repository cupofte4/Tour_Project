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

    private readonly IStallApiClient _stallApiClient;
    private readonly INarrationService _narrationService;
    private bool _hasLoaded;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private StallSummary? _selectedStall;
    private LanguageOption? _selectedPreviewLanguage;
    private string _previewStatusText = "Ready to preview narration.";

    public StallListViewModel(IStallApiClient stallApiClient, INarrationService narrationService)
    {
        _stallApiClient = stallApiClient;
        _narrationService = narrationService;
        _narrationService.PlaybackStateChanged += OnNarrationPlaybackStateChanged;

        var defaultLanguageCode = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(CultureInfo.CurrentUICulture.Name);
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
            var stalls = await _stallApiClient.GetStallsAsync();

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

    public async Task PreviewNarrationAsync(StallSummary? stall)
    {
        if (stall is null)
        {
            PreviewStatusText = "Preview unavailable.";
            return;
        }

        var requestedLanguage = SelectedPreviewLanguage?.Code ?? "en";
        var resolvedNarration = await StallNarrationResolver.ResolveAsync(
            requestedLanguage,
            stall.Name,
            StallNarrationResolver.ResolveVietnameseNarrationText(stall.NarrationScriptVi, stall.DescriptionVi),
            stall.AudioUrl,
            languageCode => Task.FromResult(stall.Translations.FirstOrDefault(item =>
                string.Equals(item.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase))));

        if (!resolvedNarration.CanNarrate)
        {
            PreviewStatusText = $"Preview unavailable for {stall.Name}.";
            return;
        }

        PreviewStatusText = $"Previewing {resolvedNarration.LanguageCode.ToUpperInvariant()} for {stall.Name}.";

        await _narrationService.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = stall.Id,
            Priority = stall.Priority,
            Title = resolvedNarration.Name,
            AudioUrl = resolvedNarration.AudioUrl,
            Text = resolvedNarration.Text,
            LanguageCode = resolvedNarration.LanguageCode
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
            NarrationPlaybackStatus.Idle => "Ready to preview narration.",
            NarrationPlaybackStatus.Queued => "Preview queued...",
            NarrationPlaybackStatus.Preparing => "Preparing preview...",
            NarrationPlaybackStatus.Playing => "Preview playing...",
            NarrationPlaybackStatus.Stopped => string.IsNullOrWhiteSpace(state.Message)
                ? "Preview stopped."
                : state.Message,
            NarrationPlaybackStatus.Error => string.IsNullOrWhiteSpace(state.Message)
                ? "Preview failed."
                : state.Message,
            _ => "Ready to preview narration."
        };
    }
}
