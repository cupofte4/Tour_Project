using System.Collections.ObjectModel;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ITtsSettingsSupportService _ttsSettingsSupportService;
    private readonly ILocationTrackingService _locationTrackingService;
    private bool _isLoading;
    private bool _isSaving;
    private bool _hasLoaded;
    private bool _isGpsTrackingEnabled;
    private double _triggerRadiusMultiplier = 1.0d;
    private bool _autoNarrationEnabled = true;
    private TtsLanguageOption? _selectedLanguage;
    private string _preferredTtsVoiceId = string.Empty;
    private string _statusMessage = string.Empty;
    private string _supportNote = string.Empty;

    public SettingsViewModel(
        ISettingsService settingsService,
        ITtsSettingsSupportService ttsSettingsSupportService,
        ILocationTrackingService locationTrackingService)
    {
        _settingsService = settingsService;
        _ttsSettingsSupportService = ttsSettingsSupportService;
        _locationTrackingService = locationTrackingService;
    }

    public ObservableCollection<TtsLanguageOption> SupportedLanguages { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

    public bool IsGpsTrackingEnabled
    {
        get => _isGpsTrackingEnabled;
        set => SetProperty(ref _isGpsTrackingEnabled, value);
    }

    public double TriggerRadiusMultiplier
    {
        get => _triggerRadiusMultiplier;
        set
        {
            if (SetProperty(ref _triggerRadiusMultiplier, Math.Round(value, 2, MidpointRounding.AwayFromZero)))
            {
                OnPropertyChanged(nameof(TriggerRadiusMultiplierText));
            }
        }
    }

    public string TriggerRadiusMultiplierText => $"{TriggerRadiusMultiplier:0.0}x";

    public bool AutoNarrationEnabled
    {
        get => _autoNarrationEnabled;
        set => SetProperty(ref _autoNarrationEnabled, value);
    }

    public TtsLanguageOption? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                UpdateSupportNote();
            }
        }
    }

    public string PreferredTtsVoiceId
    {
        get => _preferredTtsVoiceId;
        set
        {
            if (SetProperty(ref _preferredTtsVoiceId, value))
            {
                UpdateSupportNote();
            }
        }
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

    public string SupportNote
    {
        get => _supportNote;
        private set
        {
            if (SetProperty(ref _supportNote, value))
            {
                OnPropertyChanged(nameof(HasSupportNote));
            }
        }
    }

    public bool HasSupportNote => !string.IsNullOrWhiteSpace(SupportNote);

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
            var settings = _settingsService.GetSettings();
            var supportedLanguages = await _ttsSettingsSupportService.GetSupportedLanguagesAsync();

            SupportedLanguages.Clear();
            foreach (var language in supportedLanguages)
            {
                SupportedLanguages.Add(language);
            }

            var selectedLanguage = SupportedLanguages.FirstOrDefault(option =>
                string.Equals(option.Code, settings.PreferredTtsLanguage, StringComparison.OrdinalIgnoreCase));

            if (selectedLanguage is null && !string.IsNullOrWhiteSpace(settings.PreferredTtsLanguage))
            {
                selectedLanguage = new TtsLanguageOption
                {
                    Code = settings.PreferredTtsLanguage,
                    DisplayName = $"{settings.PreferredTtsLanguage} (Unavailable)"
                };
                SupportedLanguages.Add(selectedLanguage);
            }

            IsGpsTrackingEnabled = settings.IsGpsTrackingEnabled;
            TriggerRadiusMultiplier = settings.TriggerRadiusMultiplier;
            AutoNarrationEnabled = settings.AutoNarrationEnabled;
            PreferredTtsVoiceId = settings.PreferredTtsVoiceId;
            SelectedLanguage = selectedLanguage ?? SupportedLanguages.FirstOrDefault();
            _hasLoaded = true;
            UpdateSupportNote();
        }
        catch (Exception)
        {
            StatusMessage = "Could not load settings.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SaveAsync()
    {
        if (IsSaving)
        {
            return;
        }

        IsSaving = true;
        StatusMessage = string.Empty;

        try
        {
            _settingsService.SaveSettings(new AppSettings
            {
                IsGpsTrackingEnabled = IsGpsTrackingEnabled,
                TriggerRadiusMultiplier = TriggerRadiusMultiplier,
                AutoNarrationEnabled = AutoNarrationEnabled,
                PreferredTtsLanguage = SelectedLanguage?.Code ?? string.Empty,
                PreferredTtsVoiceId = PreferredTtsVoiceId
            });

            if (!IsGpsTrackingEnabled && _locationTrackingService.IsTracking)
            {
                await _locationTrackingService.StopTrackingAsync();
            }

            StatusMessage = "Settings saved.";
            UpdateSupportNote();
        }
        catch (Exception)
        {
            StatusMessage = "Could not save settings.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void UpdateSupportNote()
    {
        if (SelectedLanguage is not null &&
            !string.IsNullOrWhiteSpace(SelectedLanguage.Code) &&
            SelectedLanguage.DisplayName.EndsWith("(Unavailable)", StringComparison.OrdinalIgnoreCase))
        {
            SupportNote = "This language may not be available on the current device. The system voice will be used if needed.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(PreferredTtsVoiceId))
        {
            SupportNote = "Specific voice IDs vary by device and may be ignored by the current MAUI text-to-speech API.";
            return;
        }

        SupportNote = string.Empty;
    }
}
