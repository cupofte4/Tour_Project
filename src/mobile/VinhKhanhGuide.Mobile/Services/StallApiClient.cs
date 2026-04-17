using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public class StallApiClient : IStallApiClient
{
    private readonly HttpClient _httpClient;
    private readonly StallApiOptions _options;
    private readonly ISettingsService _settingsService;

    public StallApiClient(HttpClient httpClient, IOptions<StallApiOptions> options, ISettingsService settingsService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _settingsService = settingsService;
    }

    public async Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
    {
        var stalls = await _httpClient.GetFromJsonAsync<List<StallSummary>>(CreateUri("api/stalls"), cancellationToken);

        return stalls ?? [];
    }

    public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
    {
        return _httpClient.GetFromJsonAsync<StallDetail>(CreateUri($"api/stalls/{stallId}"), cancellationToken);
    }

    public async Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            CreateUri($"api/stalls/{stallId}/translation?lang={Uri.EscapeDataString(languageCode)}"),
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StallTranslation>(cancellationToken);
    }

    private Uri CreateUri(string relativePath)
    {
        var baseUrl = _settingsService.GetResolvedApiBaseUrl();

        if (!StallApiOptions.TryNormalizeBaseUrl(baseUrl, out var normalizedBaseUrl))
        {
            if (!StallApiOptions.TryNormalizeBaseUrl(_options.BaseUrl, out normalizedBaseUrl))
            {
                throw new InvalidOperationException("Stall API base URL is not configured.");
            }
        }

        return new Uri(new Uri(normalizedBaseUrl, UriKind.Absolute), relativePath);
    }
}
