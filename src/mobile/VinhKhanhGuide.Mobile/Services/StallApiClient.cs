using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public class StallApiClient : IStallApiClient
{
    private readonly HttpClient _httpClient;

    public StallApiClient(IOptions<StallApiOptions> options)
    {
        var baseUrl = options.Value.BaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Stall API base URL is not configured.");
        }

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute)
        };
    }

    public async Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
    {
        var stalls = await _httpClient.GetFromJsonAsync<List<StallSummary>>("api/stalls", cancellationToken);

        return stalls ?? [];
    }

    public Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default)
    {
        return _httpClient.GetFromJsonAsync<StallDetail>($"api/stalls/{stallId}", cancellationToken);
    }

    public Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        return _httpClient.GetFromJsonAsync<StallTranslation>(
            $"api/stalls/{stallId}/translation?lang={Uri.EscapeDataString(languageCode)}",
            cancellationToken);
    }
}
