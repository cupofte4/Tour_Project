using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class AnalyticsApiClient : IAnalyticsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly StallApiOptions _stallApiOptions;
    private readonly ISettingsService _settingsService;

    public AnalyticsApiClient(
        HttpClient httpClient,
        IOptions<StallApiOptions> stallApiOptions,
        ISettingsService settingsService)
    {
        _httpClient = httpClient;
        _stallApiOptions = stallApiOptions.Value;
        _settingsService = settingsService;
    }

    public async Task PostEventAsync(AppUsageEventRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            CreateUri("api/analytics/events"),
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task PostHeartbeatAsync(AppUsageHeartbeatRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            CreateUri("api/analytics/heartbeat"),
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private Uri CreateUri(string relativePath)
    {
        var baseUrl = _settingsService.GetResolvedApiBaseUrl();

        if (!StallApiOptions.TryNormalizeBaseUrl(baseUrl, out var normalizedBaseUrl))
        {
            if (!StallApiOptions.TryNormalizeBaseUrl(_stallApiOptions.BaseUrl, out normalizedBaseUrl))
            {
                throw new InvalidOperationException("Stall API base URL is not configured.");
            }
        }

        return new Uri(new Uri(normalizedBaseUrl, UriKind.Absolute), relativePath);
    }
}
