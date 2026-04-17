using System.Globalization;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class ProximityNarrationCoordinator : IProximityNarrationCoordinator
{
    private readonly object _syncRoot = new();
    private readonly IStallDataService _stallDataService;
    private readonly INarrationService _narrationService;
    private readonly ISettingsService _settingsService;
    private readonly AutoNarrationOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<int, DateTimeOffset> _lastNarratedAt = [];
    private NearbyStallNotification? _currentPrompt;

    public static ProximityNarrationCoordinator CreateForApiClient(
        IStallApiClient stallApiClient,
        INarrationService narrationService,
        ISettingsService settingsService,
        IOptions<AutoNarrationOptions> options,
        TimeProvider timeProvider)
    {
        return new ProximityNarrationCoordinator(
            new DirectStallDataService(stallApiClient),
            narrationService,
            settingsService,
            options,
            timeProvider);
    }

    public ProximityNarrationCoordinator(
        IStallDataService stallDataService,
        INarrationService narrationService,
        ISettingsService settingsService,
        IOptions<AutoNarrationOptions> options,
        TimeProvider timeProvider)
    {
        _stallDataService = stallDataService;
        _narrationService = narrationService;
        _settingsService = settingsService;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public NearbyStallNotification? CurrentPrompt
    {
        get
        {
            lock (_syncRoot)
            {
                return _currentPrompt;
            }
        }
    }

    public async Task<NearbyStallNotification?> HandleTriggerAsync(
        NearbyStallNotification notification,
        IEnumerable<StallSummary> stalls,
        CancellationToken cancellationToken = default)
    {
        var stall = stalls.FirstOrDefault(item => item.Id == notification.StallId);

        if (stall is null)
        {
            return CurrentPrompt;
        }

        var localizedContent = await ResolveLocalizedContentAsync(stall, cancellationToken);

        NearbyStallNotification prompt;
        bool shouldStartNarration;
        NarrationRequest? narrationRequest = null;

        lock (_syncRoot)
        {
            var now = GetNow();
            var settings = _settingsService.GetSettings();
            var activeState = _narrationService.CurrentState;
            var isSamePoiAlreadyNarrating = activeState.ActivePoiId == stall.Id && _narrationService.IsPlaying;
            var isInsideReplayCooldown = _lastNarratedAt.TryGetValue(stall.Id, out var lastNarratedAt) &&
                                         now - lastNarratedAt < _options.ReplayCooldown;

            shouldStartNarration = settings.AutoNarrationEnabled &&
                                   !isInsideReplayCooldown &&
                                   !isSamePoiAlreadyNarrating;

            prompt = CreatePrompt(
                localizedContent,
                notification,
                canStartNarration: !shouldStartNarration && !isSamePoiAlreadyNarrating,
                autoPlayStarted: shouldStartNarration,
                promptText: ResolvePromptText(shouldStartNarration, isSamePoiAlreadyNarrating, isInsideReplayCooldown));

            _currentPrompt = MergePrompt(prompt);

            if (shouldStartNarration)
            {
                _lastNarratedAt[stall.Id] = now;
                narrationRequest = CreateNarrationRequest(stall, localizedContent);
            }
        }

        if (narrationRequest is not null)
        {
            await _narrationService.RequestNarrationAsync(narrationRequest, cancellationToken);
        }

        return CurrentPrompt;
    }

    public NearbyStallNotification? DismissPrompt()
    {
        lock (_syncRoot)
        {
            _currentPrompt = null;
            return null;
        }
    }

    public async Task<NearbyStallNotification?> StartPromptNarrationAsync(
        IEnumerable<StallSummary> stalls,
        CancellationToken cancellationToken = default)
    {
        NearbyStallNotification? prompt;
        StallSummary? stall;
        NarrationRequest? narrationRequest = null;

        lock (_syncRoot)
        {
            prompt = _currentPrompt;
            stall = prompt is null
                ? null
                : stalls.FirstOrDefault(item => item.Id == prompt.StallId);

            if (prompt is null || stall is null)
            {
                return _currentPrompt;
            }

            _lastNarratedAt[stall.Id] = GetNow();
            _currentPrompt = CreatePrompt(
                new ResolvedStallNarration(
                    prompt.StallName,
                    prompt.NarrationText,
                    prompt.RequestedLanguageCode,
                    prompt.LanguageCode,
                    prompt.LocaleCode,
                    string.Empty,
                    !string.IsNullOrWhiteSpace(prompt.NarrationText),
                    !string.IsNullOrWhiteSpace(prompt.NarrationText),
                    prompt.UsedFallback),
                prompt,
                canStartNarration: false,
                autoPlayStarted: true,
                promptText: "Playing nearby narration.");
            narrationRequest = CreateNarrationRequest(
                stall,
                new ResolvedStallNarration(
                    prompt.StallName,
                    prompt.NarrationText,
                    prompt.RequestedLanguageCode,
                    prompt.LanguageCode,
                    prompt.LocaleCode,
                    string.Empty,
                    !string.IsNullOrWhiteSpace(prompt.NarrationText),
                    !string.IsNullOrWhiteSpace(prompt.NarrationText),
                    prompt.UsedFallback));
        }

        if (narrationRequest is not null)
        {
            await _narrationService.RequestNarrationAsync(narrationRequest, cancellationToken);
        }

        return CurrentPrompt;
    }

    private DateTimeOffset GetNow()
    {
        return _timeProvider.GetUtcNow();
    }

    private async Task<ResolvedStallNarration> ResolveLocalizedContentAsync(
        StallSummary stall,
        CancellationToken cancellationToken)
    {
        var requestedLanguage = ResolveRequestedLanguage();

        return await StallNarrationResolver.ResolveAsync(
            requestedLanguage,
            stall.Name,
            ResolveVietnameseNarrationText(stall),
            ResolvePreferredAudioSource(stall),
            async candidateLanguage =>
            {
                var translation = stall.Translations.FirstOrDefault(item =>
                    string.Equals(item.LanguageCode, candidateLanguage, StringComparison.OrdinalIgnoreCase));

                translation ??= await _stallDataService.GetTranslationAsync(stall.Id, candidateLanguage, cancellationToken);
                return translation;
            });
    }

    private string ResolveRequestedLanguage()
    {
        var settings = _settingsService.GetSettings();
        var preferredLanguage = !string.IsNullOrWhiteSpace(settings.PreferredTtsLanguage)
            ? settings.PreferredTtsLanguage
            : CultureInfo.CurrentUICulture.Name;

        return TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(preferredLanguage);
    }

    private static NarrationRequest CreateNarrationRequest(StallSummary stall, ResolvedStallNarration content)
    {
        return new NarrationRequest
        {
            PoiId = stall.Id,
            Priority = stall.Priority,
            Title = content.Name,
            AudioUrl = content.AudioUrl,
            Text = content.Text,
            RequestedLanguageCode = content.RequestedLanguageCode,
            LanguageCode = content.LanguageCode,
            LocaleCode = content.LocaleCode,
            UsedFallback = content.UsedFallback
        };
    }

    private static string ResolvePreferredAudioSource(StallSummary stall)
    {
        return NarrationAudioSourceResolver.ResolvePreferredAudioSource(stall.LocalAudioPath, stall.AudioUrl);
    }

    private NearbyStallNotification MergePrompt(NearbyStallNotification nextPrompt)
    {
        if (_currentPrompt is not null &&
            _currentPrompt.StallId == nextPrompt.StallId &&
            _currentPrompt.CanStartNarration == nextPrompt.CanStartNarration &&
            _currentPrompt.AutoPlayStarted == nextPrompt.AutoPlayStarted &&
            string.Equals(_currentPrompt.NarrationText, nextPrompt.NarrationText, StringComparison.Ordinal) &&
            string.Equals(_currentPrompt.RequestedLanguageCode, nextPrompt.RequestedLanguageCode, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(_currentPrompt.LanguageCode, nextPrompt.LanguageCode, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(_currentPrompt.LocaleCode, nextPrompt.LocaleCode, StringComparison.OrdinalIgnoreCase) &&
            _currentPrompt.UsedFallback == nextPrompt.UsedFallback &&
            string.Equals(_currentPrompt.PromptText, nextPrompt.PromptText, StringComparison.Ordinal))
        {
            return _currentPrompt;
        }

        return nextPrompt;
    }

    private static NearbyStallNotification CreatePrompt(
        ResolvedStallNarration content,
        NearbyStallNotification source,
        bool canStartNarration,
        bool autoPlayStarted,
        string promptText)
    {
        return new NearbyStallNotification
        {
            StallId = source.StallId,
            StallName = content.Name,
            Category = source.Category,
            ImageUrl = source.ImageUrl,
            DistanceMeters = source.DistanceMeters,
            TriggerReason = source.TriggerReason,
            Timestamp = source.Timestamp,
            CanStartNarration = canStartNarration,
            AutoPlayStarted = autoPlayStarted,
            PromptText = promptText,
            NarrationText = content.Text,
            RequestedLanguageCode = content.RequestedLanguageCode,
            LanguageCode = content.LanguageCode,
            LocaleCode = content.LocaleCode,
            UsedFallback = content.UsedFallback,
            PlaybackStatusText = autoPlayStarted ? "Playing" : string.Empty
        };
    }

    private static string ResolvePromptText(
        bool shouldStartNarration,
        bool isSamePoiAlreadyNarrating,
        bool isInsideReplayCooldown)
    {
        if (shouldStartNarration)
        {
            return "Playing nearby narration.";
        }

        if (isSamePoiAlreadyNarrating)
        {
            return "This POI is already playing.";
        }

        if (isInsideReplayCooldown)
        {
            return "Nearby again. Tap to listen when you want.";
        }

        return "Nearby POI. Tap to listen or open details.";
    }

    private static string ResolveVietnameseNarrationText(StallSummary stall)
    {
        return !string.IsNullOrWhiteSpace(stall.NarrationScriptVi)
            ? stall.NarrationScriptVi
            : stall.DescriptionVi;
    }

}
