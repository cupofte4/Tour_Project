using System.Diagnostics;
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

    private readonly IStallDataService _stallDataService;
    private readonly INarrationService _narrationService;
    private readonly ISettingsService _settingsService;
    private readonly IOfflineAudioDownloadService _offlineAudioDownloadService;
    private readonly IUsageAnalyticsService _usageAnalyticsService;
    private readonly Dictionary<string, StallTranslation?> _translationCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private StallDetail? _stall;
    private int _loadedStallId;
    private LanguageOption? _selectedLanguage;
    private string _displayName = string.Empty;
    private string _displayDescription = string.Empty;
    private NarrationPlaybackState _narrationState = NarrationPlaybackState.Idle;
    private ResolvedStallNarration? _resolvedNarration;
    private bool _isTranslationLoading;
    private string _lastTranslationErrorMessage = string.Empty;
    private int _translationRequestVersion;
    private Task _currentTranslationTask = Task.CompletedTask;
    private int _activeOpenRequestVersion;
    private bool _isExternalOpenInProgress;
    private bool _isAutoPlayPending;
    private bool _suppressImmediateDisplayUpdate;
    private string _pendingDisplayName = string.Empty;
    private string _pendingDisplayDescription = string.Empty;
    private string _refreshNoticeMessage = string.Empty;
    private bool _isOfflineAudioDownloading;
    private string _offlineAudioStatusMessage = string.Empty;

    public static StallDetailViewModel CreateForApiClient(
        IStallApiClient stallApiClient,
        INarrationService narrationService,
        ISettingsService settingsService)
    {
        return new StallDetailViewModel(
            new DirectStallDataService(stallApiClient),
            narrationService,
            settingsService,
            new NoopOfflineAudioDownloadService(),
            new NoopUsageAnalyticsService());
    }

    public StallDetailViewModel(
        IStallDataService stallDataService,
        INarrationService narrationService,
        ISettingsService settingsService,
        IOfflineAudioDownloadService? offlineAudioDownloadService = null,
        IUsageAnalyticsService? usageAnalyticsService = null)
    {
        _stallDataService = stallDataService;
        _narrationService = narrationService;
        _settingsService = settingsService;
        _offlineAudioDownloadService = offlineAudioDownloadService ?? new NoopOfflineAudioDownloadService();
        _usageAnalyticsService = usageAnalyticsService ?? new NoopUsageAnalyticsService();
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
                var fallbackDescription = StallNarrationResolver.ResolveVietnameseNarrationText(
                    value.NarrationScriptVi,
                    value.DescriptionVi);

                _pendingDisplayName = value.Name;
                _pendingDisplayDescription = fallbackDescription;

                if (!_suppressImmediateDisplayUpdate)
                {
                    DisplayName = _pendingDisplayName;
                    DisplayDescription = _pendingDisplayDescription;
                }

                NotifyOfflineAudioStateChanged();
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
                OnPropertyChanged(nameof(PageStatusText));
                OnPropertyChanged(nameof(HasPageStatus));
                NotifyOfflineAudioStateChanged();
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

    public string RefreshNoticeMessage
    {
        get => _refreshNoticeMessage;
        private set
        {
            if (SetProperty(ref _refreshNoticeMessage, value))
            {
                OnPropertyChanged(nameof(HasRefreshNotice));
            }
        }
    }

    public bool HasRefreshNotice => !string.IsNullOrWhiteSpace(RefreshNoticeMessage);

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
                _currentTranslationTask = ApplySelectedLanguageAsync(_loadedStallId, value.Code);
            }
        }
    }

    public bool IsTranslationLoading
    {
        get => _isTranslationLoading;
        private set
        {
            if (SetProperty(ref _isTranslationLoading, value))
            {
                OnPropertyChanged(nameof(CanStartNarration));
                OnPropertyChanged(nameof(PageStatusText));
                OnPropertyChanged(nameof(HasPageStatus));
                OnPropertyChanged(nameof(TranslationStatusText));
                OnPropertyChanged(nameof(HasTranslationStatus));
            }
        }
    }

    public string LastTranslationErrorMessage
    {
        get => _lastTranslationErrorMessage;
        private set
        {
            if (SetProperty(ref _lastTranslationErrorMessage, value))
            {
                OnPropertyChanged(nameof(HasTranslationError));
                OnPropertyChanged(nameof(TranslationNoticeText));
                OnPropertyChanged(nameof(HasTranslationNotice));
            }
        }
    }

    public bool HasTranslationError => !string.IsNullOrWhiteSpace(LastTranslationErrorMessage);

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
        _ when IsAutoPlayPending => "Preparing narration...",
        NarrationPlaybackStatus.Idle when IsNarrationAvailable => "Ready to listen to narration.",
        NarrationPlaybackStatus.Idle => "Narration is not available for this location.",
        NarrationPlaybackStatus.Queued => "Starting narration...",
        NarrationPlaybackStatus.Preparing => "Preparing narration...",
        NarrationPlaybackStatus.Playing => "Narration is playing.",
        NarrationPlaybackStatus.Error => string.IsNullOrWhiteSpace(NarrationState.Message)
            ? "We couldn't start narration."
            : NarrationState.Message,
        NarrationPlaybackStatus.Stopped => string.IsNullOrWhiteSpace(NarrationState.Message)
            ? "Narration stopped."
            : NarrationState.Message,
        _ => string.Empty
    };

    public bool HasNarrationStatus => true;

    public bool IsNarrationAvailable => _resolvedNarration?.CanNarrate == true;

    public string ResolvedNarrationLanguageCode => _resolvedNarration?.LanguageCode ?? string.Empty;

    public string RequestedNarrationLanguageCode => _resolvedNarration?.RequestedLanguageCode ?? SelectedLanguage?.Code ?? string.Empty;

    public bool IsNarrationUsingFallbackLanguage => _resolvedNarration?.UsedFallback == true;

    public bool IsUsingFallbackTranslation => _resolvedNarration?.UsedFallback == true;

    public string ResolvedDisplayLanguageCode => _resolvedNarration?.LanguageCode ?? string.Empty;

    public Task CurrentTranslationTask => _currentTranslationTask;

    public string PageStatusText
    {
        get
        {
            if (IsExternalOpenInProgress && IsLoading)
            {
                return "Opening location...";
            }

            if (IsExternalOpenInProgress && IsTranslationLoading)
            {
                return "Opening location...";
            }

            if (IsTranslationLoading)
            {
                return "Loading translation...";
            }

            if (IsLoading)
            {
                return "Loading location...";
            }

            if (IsAutoPlayPending)
            {
                return "Preparing narration...";
            }

            return string.Empty;
        }
    }

    public bool HasPageStatus => !string.IsNullOrWhiteSpace(PageStatusText);

    public string TranslationStatusText => IsTranslationLoading
        ? "Loading translation..."
        : string.Empty;

    public bool HasTranslationStatus => !string.IsNullOrWhiteSpace(TranslationStatusText);

    public string TranslationNoticeText => HasTranslationError
        ? LastTranslationErrorMessage
        : IsUsingFallbackTranslation && !string.Equals(SelectedLanguage?.Code, "vi", StringComparison.OrdinalIgnoreCase)
            ? "This language is not available yet, so Vietnamese is being used."
            : string.Empty;

    public bool HasTranslationNotice => !string.IsNullOrWhiteSpace(TranslationNoticeText);

    public bool IsExternalOpenInProgress
    {
        get => _isExternalOpenInProgress;
        private set
        {
            if (SetProperty(ref _isExternalOpenInProgress, value))
            {
                OnPropertyChanged(nameof(NarrationStatusText));
                OnPropertyChanged(nameof(HasNarrationStatus));
                OnPropertyChanged(nameof(PageStatusText));
                OnPropertyChanged(nameof(HasPageStatus));
            }
        }
    }

    public bool IsAutoPlayPending
    {
        get => _isAutoPlayPending;
        private set
        {
            if (SetProperty(ref _isAutoPlayPending, value))
            {
                OnPropertyChanged(nameof(NarrationStatusText));
                OnPropertyChanged(nameof(HasNarrationStatus));
                OnPropertyChanged(nameof(PageStatusText));
                OnPropertyChanged(nameof(HasPageStatus));
            }
        }
    }

    public bool CanStartNarration => Stall is not null &&
                                     IsNarrationAvailable &&
                                     !IsLoading &&
                                     !IsTranslationLoading &&
                                     (!IsNarrating || NarrationState.ActivePoiId != Stall.Id);

    public bool CanStopNarration => IsNarrating && Stall is not null && NarrationState.ActivePoiId == Stall.Id;

    public bool IsOfflineAudioDownloading
    {
        get => _isOfflineAudioDownloading;
        private set
        {
            if (SetProperty(ref _isOfflineAudioDownloading, value))
            {
                NotifyOfflineAudioStateChanged();
            }
        }
    }

    public string OfflineAudioStatusText
    {
        get
        {
            if (IsOfflineAudioDownloading)
            {
                return "Downloading...";
            }

            if (HasOfflineAudio)
            {
                return "Available offline";
            }

            if (!string.IsNullOrWhiteSpace(_offlineAudioStatusMessage))
            {
                return _offlineAudioStatusMessage;
            }

            return IsOfflineAudioDownloadAvailable
                ? "Download for offline use"
                : "Offline audio unavailable";
        }
    }

    public bool HasOfflineAudioStatus => true;

    public bool IsOfflineAudioDownloadAvailable => Stall is not null && _offlineAudioDownloadService.CanDownloadAudio(Stall);

    public bool HasOfflineAudio => Stall is not null &&
                                   NarrationAudioSourceResolver.TryGetValidLocalAudioPath(Stall.LocalAudioPath, out _);

    public bool CanDownloadOfflineAudio => Stall is not null &&
                                           IsOfflineAudioDownloadAvailable &&
                                           !HasOfflineAudio &&
                                           !IsOfflineAudioDownloading &&
                                           !IsLoading;

    public bool CanRemoveOfflineAudio => Stall is not null &&
                                         HasOfflineAudio &&
                                         !IsOfflineAudioDownloading;

    public async Task LoadAsync(int stallId, string? preferredLanguageCode = null)
    {
        if (stallId <= 0 || (_loadedStallId == stallId && Stall is not null) || IsLoading)
        {
            if (stallId > 0 && Stall is not null && !string.IsNullOrWhiteSpace(preferredLanguageCode))
            {
                await ApplyPreferredLanguageAsync(stallId, preferredLanguageCode);
            }

            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        RefreshNoticeMessage = string.Empty;

        try
        {
            var cachedStall = await _stallDataService.GetCachedStallDetailAsync(stallId);
            if (cachedStall is not null)
            {
                Stall = cachedStall;
                await EnsureLocalAudioPathIsValidAsync();
                _loadedStallId = stallId;
                _translationCache.Clear();
                RefreshNoticeMessage = "Showing saved details while refreshing.";
            }

            var refreshedStall = await _stallDataService.RefreshStallDetailAsync(stallId);
            Stall = refreshedStall ?? cachedStall;
            await EnsureLocalAudioPathIsValidAsync();
            _loadedStallId = stallId;
            _translationCache.Clear();
            RefreshNoticeMessage = string.Empty;

            if (Stall is null)
            {
                ErrorMessage = "We couldn't open that location.";
            }
            else
            {
                await ApplyInitialLanguageSelectionAsync(stallId, preferredLanguageCode);
            }
        }
        catch (Exception)
        {
            if (Stall is not null)
            {
                RefreshNoticeMessage = "Showing saved details because the latest content couldn't be refreshed.";
                await ApplyInitialLanguageSelectionAsync(stallId, preferredLanguageCode);
            }
            else
            {
                ErrorMessage = "We couldn't load that location right now.";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task OpenAsync(
        int stallId,
        string? preferredLanguageCode = null,
        bool autoPlay = false,
        int requestVersion = 0)
    {
        _activeOpenRequestVersion = requestVersion;
        IsExternalOpenInProgress = true;
        IsAutoPlayPending = autoPlay;
        _suppressImmediateDisplayUpdate = !string.IsNullOrWhiteSpace(preferredLanguageCode);

        if (_suppressImmediateDisplayUpdate)
        {
            DisplayName = string.Empty;
            DisplayDescription = string.Empty;
        }

        try
        {
            await LoadAsync(stallId, preferredLanguageCode);

            if (requestVersion != 0 && requestVersion != _activeOpenRequestVersion)
            {
                return;
            }

            await _currentTranslationTask;

            if (HasError || Stall is null)
            {
                return;
            }

            _ = _usageAnalyticsService.TrackStallViewAsync(stallId);
            IsExternalOpenInProgress = false;

            if (!autoPlay || !CanStartNarration)
            {
                IsAutoPlayPending = false;
                return;
            }

            await StartNarrationAsync();
        }
        finally
        {
            if (HasError || Stall is null || !autoPlay)
            {
                IsExternalOpenInProgress = false;
                IsAutoPlayPending = false;
            }

            _suppressImmediateDisplayUpdate = false;
        }
    }

    private async Task ApplySelectedLanguageAsync(int stallId, string languageCode)
    {
        var requestVersion = Interlocked.Increment(ref _translationRequestVersion);
        var normalizedLanguageCode = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(languageCode);

        await InvalidateActiveNarrationForLanguageChangeAsync(normalizedLanguageCode);
        await LoadTranslationAsync(stallId, normalizedLanguageCode, requestVersion);
    }

    private async Task ApplyPreferredLanguageAsync(int stallId, string preferredLanguageCode)
    {
        var normalizedLanguageCode = NormalizeRequestedLanguage(preferredLanguageCode);
        if (string.IsNullOrWhiteSpace(normalizedLanguageCode))
        {
            if (SelectedLanguage is null)
            {
                var defaultLanguageCode = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(CultureInfo.CurrentUICulture.Name);
                _selectedLanguage = Languages.First(language =>
                    string.Equals(language.Code, defaultLanguageCode, StringComparison.OrdinalIgnoreCase));
                OnPropertyChanged(nameof(SelectedLanguage));
            }

            _currentTranslationTask = ApplySelectedLanguageAsync(stallId, SelectedLanguage?.Code ?? "vi");
            await _currentTranslationTask;
            return;
        }

        var matchingLanguage = Languages.First(language =>
            string.Equals(language.Code, normalizedLanguageCode, StringComparison.OrdinalIgnoreCase));

        _selectedLanguage = matchingLanguage;
        OnPropertyChanged(nameof(SelectedLanguage));
        _currentTranslationTask = ApplySelectedLanguageAsync(stallId, matchingLanguage.Code);
        await _currentTranslationTask;
    }

    private async Task ApplyInitialLanguageSelectionAsync(int stallId, string? preferredLanguageCode)
    {
        if (!string.IsNullOrWhiteSpace(preferredLanguageCode))
        {
            await ApplyPreferredLanguageAsync(stallId, preferredLanguageCode);
            return;
        }

        if (SelectedLanguage is null)
        {
            var persistedLanguageCode = _settingsService.GetSettings().PreferredTtsLanguage;
            await ApplyPreferredLanguageAsync(stallId, persistedLanguageCode);
            return;
        }

        _currentTranslationTask = ApplySelectedLanguageAsync(stallId, SelectedLanguage.Code);
        await _currentTranslationTask;
    }

    private async Task LoadTranslationAsync(int stallId, string languageCode, int requestVersion)
    {
        if (Stall is null)
        {
            return;
        }

        IsTranslationLoading = true;
        LastTranslationErrorMessage = string.Empty;

        try
        {
            StallTranslation? selectedTranslation = null;
            var translationLoadFailed = false;
            string translationErrorMessage = string.Empty;

            if (!string.Equals(languageCode, "vi", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    selectedTranslation = await GetOrLoadTranslationAsync(stallId, languageCode);
                }
                catch (Exception ex)
                {
                    translationLoadFailed = true;
                    translationErrorMessage = "This language is not available yet, so Vietnamese is being used.";
                    Debug.WriteLine($"Translation load failed for stall {stallId}, language {languageCode}: {ex}");
                }
            }

            var resolvedContent = await ResolveLocalizedContentAsync(
                stallId,
                languageCode,
                selectedTranslation,
                translationLoadFailed);

            if (requestVersion != _translationRequestVersion)
            {
                return;
            }

            ApplyResolvedDisplayContent(resolvedContent.Name, resolvedContent.Text);
            _resolvedNarration = resolvedContent;
            LastTranslationErrorMessage = translationErrorMessage;
            OnPropertyChanged(nameof(IsNarrationAvailable));
            OnPropertyChanged(nameof(CanStartNarration));
            OnPropertyChanged(nameof(NarrationStatusText));
            OnPropertyChanged(nameof(ResolvedNarrationLanguageCode));
            OnPropertyChanged(nameof(RequestedNarrationLanguageCode));
            OnPropertyChanged(nameof(IsNarrationUsingFallbackLanguage));
            OnPropertyChanged(nameof(IsUsingFallbackTranslation));
            OnPropertyChanged(nameof(ResolvedDisplayLanguageCode));
            OnPropertyChanged(nameof(TranslationNoticeText));
            OnPropertyChanged(nameof(HasTranslationNotice));
        }
        catch (Exception ex)
        {
            if (requestVersion != _translationRequestVersion)
            {
                return;
            }

            ApplyResolvedDisplayContent(
                Stall.Name,
                StallNarrationResolver.ResolveVietnameseNarrationText(
                    Stall.NarrationScriptVi,
                    Stall.DescriptionVi));
            _resolvedNarration = await StallNarrationResolver.ResolveAsync(
                languageCode,
                Stall.Name,
                DisplayDescription,
                ResolvePreferredAudioSource(Stall),
                _ => Task.FromResult<StallTranslation?>(null));
            LastTranslationErrorMessage = "This language is not available yet, so Vietnamese is being used.";
            Debug.WriteLine($"Translation resolution failed for stall {stallId}, language {languageCode}: {ex}");
            OnPropertyChanged(nameof(IsNarrationAvailable));
            OnPropertyChanged(nameof(CanStartNarration));
            OnPropertyChanged(nameof(NarrationStatusText));
            OnPropertyChanged(nameof(ResolvedNarrationLanguageCode));
            OnPropertyChanged(nameof(RequestedNarrationLanguageCode));
            OnPropertyChanged(nameof(IsNarrationUsingFallbackLanguage));
            OnPropertyChanged(nameof(IsUsingFallbackTranslation));
            OnPropertyChanged(nameof(ResolvedDisplayLanguageCode));
            OnPropertyChanged(nameof(TranslationNoticeText));
            OnPropertyChanged(nameof(HasTranslationNotice));
        }
        finally
        {
            if (requestVersion == _translationRequestVersion)
            {
                IsTranslationLoading = false;
            }
        }
    }

    public async Task StartNarrationAsync()
    {
        if (Stall is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(Stall.LocalAudioPath) &&
            !NarrationAudioSourceResolver.TryGetValidLocalAudioPath(Stall.LocalAudioPath, out _))
        {
            await UpdateStallLocalAudioPathAsync(string.Empty);
        }

        await _narrationService.RequestNarrationAsync(new NarrationRequest
        {
            PoiId = Stall.Id,
            Priority = Stall.Priority,
            Title = DisplayName,
            AudioUrl = _resolvedNarration?.AudioUrl ?? ResolvePreferredAudioSource(Stall),
            Text = _resolvedNarration?.Text ?? ResolveNarrationText(),
            RequestedLanguageCode = _resolvedNarration?.RequestedLanguageCode ?? SelectedLanguage?.Code ?? "en",
            LanguageCode = _resolvedNarration?.LanguageCode ?? SelectedLanguage?.Code ?? "en",
            LocaleCode = _resolvedNarration?.LocaleCode
                ?? TextToSpeechSettingsResolver.ResolvePreferredLocaleHint(_resolvedNarration?.LanguageCode ?? SelectedLanguage?.Code),
            UsedFallback = _resolvedNarration?.UsedFallback == true
        });
    }

    public Task StopNarrationAsync()
    {
        return _narrationService.StopAsync();
    }

    public async Task DownloadOfflineAudioAsync()
    {
        if (Stall is null || !CanDownloadOfflineAudio)
        {
            return;
        }

        IsOfflineAudioDownloading = true;
        _offlineAudioStatusMessage = string.Empty;

        try
        {
            var result = await _offlineAudioDownloadService.DownloadAsync(Stall);
            if (!result.Succeeded)
            {
                _offlineAudioStatusMessage = result.Message;
                return;
            }

            await UpdateStallLocalAudioPathAsync(result.LocalAudioPath);
            _offlineAudioStatusMessage = string.Empty;
        }
        finally
        {
            IsOfflineAudioDownloading = false;
            NotifyOfflineAudioStateChanged();
        }
    }

    public async Task RemoveOfflineAudioAsync()
    {
        if (Stall is null || !CanRemoveOfflineAudio)
        {
            return;
        }

        IsOfflineAudioDownloading = true;
        _offlineAudioStatusMessage = string.Empty;

        try
        {
            await _offlineAudioDownloadService.RemoveAsync(Stall.LocalAudioPath);
            await UpdateStallLocalAudioPathAsync(string.Empty);
            _offlineAudioStatusMessage = IsOfflineAudioDownloadAvailable
                ? "Download for offline use"
                : "Offline audio unavailable";
        }
        finally
        {
            IsOfflineAudioDownloading = false;
            NotifyOfflineAudioStateChanged();
        }
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

        if (IsAutoPlayPending &&
            Stall is not null &&
            state.ActivePoiId == Stall.Id &&
            state.Status is NarrationPlaybackStatus.Playing
                or NarrationPlaybackStatus.Error
                or NarrationPlaybackStatus.Stopped
                or NarrationPlaybackStatus.Idle)
        {
            IsAutoPlayPending = false;
            IsExternalOpenInProgress = false;
        }
    }

    private async Task<ResolvedStallNarration> ResolveLocalizedContentAsync(
        int stallId,
        string requestedLanguageCode,
        StallTranslation? selectedTranslation,
        bool translationLoadFailed)
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
            ResolvePreferredAudioSource(Stall),
            async candidateLanguage =>
            {
                if (!string.Equals(candidateLanguage, requestedLanguageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (selectedTranslation is not null)
                {
                    return selectedTranslation;
                }

                if (translationLoadFailed)
                {
                    return null;
                }

                return await GetOrLoadTranslationAsync(stallId, candidateLanguage);
            });
    }

    private async Task<StallTranslation?> GetOrLoadTranslationAsync(int stallId, string languageCode)
    {
        if (_translationCache.TryGetValue(languageCode, out var cachedTranslation))
        {
            return cachedTranslation;
        }

        var existingTranslation = Stall?.Translations.FirstOrDefault(item =>
            string.Equals(item.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

        if (existingTranslation is not null)
        {
            _translationCache[languageCode] = existingTranslation;
            return existingTranslation;
        }

        var storedTranslation = await _stallDataService.GetCachedTranslationAsync(stallId, languageCode);
        if (storedTranslation is not null)
        {
            _translationCache[languageCode] = storedTranslation;
            return storedTranslation;
        }

        var loadedTranslation = await _stallDataService.GetTranslationAsync(stallId, languageCode);
        _translationCache[languageCode] = loadedTranslation;
        return loadedTranslation;
    }

    private async Task InvalidateActiveNarrationForLanguageChangeAsync(string nextLanguageCode)
    {
        var activeState = _narrationService.CurrentState;

        if (Stall is null ||
            !_narrationService.IsPlaying ||
            activeState.ActivePoiId != Stall.Id)
        {
            return;
        }

        var normalizedNextLanguage = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(nextLanguageCode);

        if (!string.Equals(activeState.RequestedLanguageCode, normalizedNextLanguage, StringComparison.OrdinalIgnoreCase))
        {
            await _narrationService.StopAsync();
        }
    }

    private static string? NormalizeRequestedLanguage(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return null;
        }

        var normalized = languageCode.Trim().ToLowerInvariant();
        return SupportedLanguages.Any(language => string.Equals(language.Code, normalized, StringComparison.OrdinalIgnoreCase))
            ? normalized
            : null;
    }

    private void ApplyResolvedDisplayContent(string name, string description)
    {
        _pendingDisplayName = name;
        _pendingDisplayDescription = description;

        if (_suppressImmediateDisplayUpdate)
        {
            _suppressImmediateDisplayUpdate = false;
        }

        DisplayName = _pendingDisplayName;
        DisplayDescription = _pendingDisplayDescription;
    }

    private static string ResolvePreferredAudioSource(StallDetail stall)
    {
        return NarrationAudioSourceResolver.ResolvePreferredAudioSource(stall.LocalAudioPath, stall.AudioUrl);
    }

    private async Task EnsureLocalAudioPathIsValidAsync()
    {
        if (Stall is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Stall.LocalAudioPath))
        {
            _offlineAudioStatusMessage = string.Empty;
            NotifyOfflineAudioStateChanged();
            return;
        }

        if (NarrationAudioSourceResolver.TryGetValidLocalAudioPath(Stall.LocalAudioPath, out _))
        {
            _offlineAudioStatusMessage = string.Empty;
            NotifyOfflineAudioStateChanged();
            return;
        }

        await UpdateStallLocalAudioPathAsync(string.Empty);
        _offlineAudioStatusMessage = IsOfflineAudioDownloadAvailable
            ? "Download for offline use"
            : "Offline audio unavailable";
        NotifyOfflineAudioStateChanged();
    }

    private async Task UpdateStallLocalAudioPathAsync(string localAudioPath)
    {
        if (Stall is null)
        {
            return;
        }

        var updatedStall = new StallDetail
        {
            Id = Stall.Id,
            Name = Stall.Name,
            DescriptionVi = Stall.DescriptionVi,
            Latitude = Stall.Latitude,
            Longitude = Stall.Longitude,
            TriggerRadiusMeters = Stall.TriggerRadiusMeters,
            Priority = Stall.Priority,
            OpenHours = Stall.OpenHours,
            Category = Stall.Category,
            ImageUrl = Stall.ImageUrl,
            MapLink = Stall.MapLink,
            NarrationScriptVi = Stall.NarrationScriptVi,
            AudioUrl = Stall.AudioUrl,
            LocalAudioPath = localAudioPath,
            IsActive = Stall.IsActive,
            AverageRating = Stall.AverageRating,
            Translations = Stall.Translations
        };

        await _stallDataService.UpdateLocalAudioPathAsync(updatedStall.Id, localAudioPath);
        Stall = updatedStall;

        if (_resolvedNarration is not null)
        {
            _resolvedNarration = _resolvedNarration with { AudioUrl = ResolvePreferredAudioSource(updatedStall) };
            OnPropertyChanged(nameof(IsNarrationAvailable));
            OnPropertyChanged(nameof(CanStartNarration));
            OnPropertyChanged(nameof(NarrationStatusText));
        }

        NotifyOfflineAudioStateChanged();
    }

    private void NotifyOfflineAudioStateChanged()
    {
        OnPropertyChanged(nameof(IsOfflineAudioDownloadAvailable));
        OnPropertyChanged(nameof(HasOfflineAudio));
        OnPropertyChanged(nameof(CanDownloadOfflineAudio));
        OnPropertyChanged(nameof(CanRemoveOfflineAudio));
        OnPropertyChanged(nameof(OfflineAudioStatusText));
        OnPropertyChanged(nameof(HasOfflineAudioStatus));
    }
}
