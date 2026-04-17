using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public sealed class StallApiClientTests
{
    [Fact]
    public async Task GetStallsAsync_UsesSettingsOverrideBeforeConfiguredBaseUrl()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            HttpResponseMessageJson<IReadOnlyList<StallSummary>>([new StallSummary { Id = 1, Name = "Stall 1" }]));
        var settingsService = new TestSettingsService
        {
            ResolvedApiBaseUrl = "https://override.example/root/"
        };
        var client = CreateClient(handler, settingsService, "https://configured.example/api/");

        var stalls = await client.GetStallsAsync();

        Assert.Single(stalls);
        Assert.Equal("https://override.example/root/api/stalls", handler.RequestUris.Single().ToString());
    }

    [Fact]
    public async Task GetStallTranslationAsync_UsesConfiguredBaseUrlWhenOverrideIsBlank()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            HttpResponseMessageJson(new StallTranslation
            {
                StallId = 7,
                LanguageCode = "en",
                Name = "English Name",
                Description = "English description"
            }));
        var settingsService = new TestSettingsService
        {
            ResolvedApiBaseUrl = string.Empty
        };
        var client = CreateClient(handler, settingsService, "https://configured.example/base/");

        var translation = await client.GetStallTranslationAsync(7, "en-US");

        Assert.NotNull(translation);
        Assert.Equal("https://configured.example/base/api/stalls/7/translation?lang=en-US", handler.RequestUris.Single().ToString());
    }

    [Fact]
    public async Task GetStallTranslationAsync_ReturnsNull_WhenApiReturnsNotFound()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = CreateClient(handler, new TestSettingsService(), "https://configured.example/base/");

        var translation = await client.GetStallTranslationAsync(7, "de");

        Assert.Null(translation);
        Assert.Equal("https://configured.example/base/api/stalls/7/translation?lang=de", handler.RequestUris.Single().ToString());
    }

    [Fact]
    public async Task GetStallsAsync_UsesConfiguredBaseUrlWhenSettingsResolveToBlank()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            HttpResponseMessageJson<IReadOnlyList<StallSummary>>([new StallSummary { Id = 2, Name = "Stall 2" }]));
        var settingsService = new TestSettingsService
        {
            ResolvedApiBaseUrl = string.Empty
        };
        var client = CreateClient(handler, settingsService, "https://ngrok-demo.example/");

        var stalls = await client.GetStallsAsync();

        Assert.Single(stalls);
        Assert.Equal("https://ngrok-demo.example/api/stalls", handler.RequestUris.Single().ToString());
    }

    [Fact]
    public async Task GetStallsAsync_ThrowsWhenNoValidBaseUrlIsConfigured()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            HttpResponseMessageJson(Array.Empty<StallSummary>()));
        var settingsService = new TestSettingsService
        {
            ResolvedApiBaseUrl = string.Empty
        };
        var client = CreateClient(handler, settingsService, string.Empty);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetStallsAsync());

        Assert.Equal("Stall API base URL is not configured.", exception.Message);
    }

    private static StallApiClient CreateClient(
        HttpMessageHandler handler,
        ISettingsService settingsService,
        string configuredBaseUrl)
    {
        return new StallApiClient(
            new HttpClient(handler),
            Options.Create(new StallApiOptions
            {
                BaseUrl = configuredBaseUrl
            }),
            settingsService);
    }

    private static HttpResponseMessage HttpResponseMessageJson<T>(T payload)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(payload)
        };
    }

    private sealed class RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<Uri> RequestUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!);
            return Task.FromResult(responseFactory(request));
        }
    }
}
