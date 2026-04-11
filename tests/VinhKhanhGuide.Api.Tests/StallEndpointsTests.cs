using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;
using VinhKhanhGuide.Infrastructure.Translations;

namespace VinhKhanhGuide.Api.Tests;

public class StallEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public StallEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStalls_ReturnsSeededStalls()
    {
        var stalls = await _client.GetFromJsonAsync<List<StallDto>>("/api/stalls");

        Assert.NotNull(stalls);
        Assert.NotEmpty(stalls);
        Assert.Equal(10, stalls.Count);
        Assert.DoesNotContain(stalls, stall => stall.Name == "Oc Co Lan");
        Assert.Contains(stalls, stall =>
            stall.Id == 1 &&
            stall.Name == "Ốc Vĩnh Khánh" &&
            stall.Address.Contains("Vĩnh Khánh") &&
            stall.ImageUrls.Count >= 1 &&
            stall.MapLink.Contains("maps.apple.com") &&
            stall.IsActive);
    }

    [Fact]
    public async Task GetStallById_ReturnsMatchingStall()
    {
        var stall = await _client.GetFromJsonAsync<StallDto>("/api/stalls/1");

        Assert.NotNull(stall);
        Assert.Equal(1, stall.Id);
        Assert.Equal("Ốc Vĩnh Khánh", stall.Name);
        Assert.Equal(10.7593, stall.Latitude);
        Assert.Equal(106.7046, stall.Longitude);
        Assert.Equal(40, stall.TriggerRadiusMeters);
        Assert.Equal(100, stall.Priority);
        Assert.Equal("Food Stall", stall.Category);
        Assert.Equal("Open hours unavailable", stall.OpenHours);
        Assert.Equal("16 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", stall.Address);
        Assert.Equal("0909123456", stall.Phone);
        Assert.NotEmpty(stall.ImageUrls);
        Assert.Equal("https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800", stall.ImageUrl);
        Assert.Contains("maps.apple.com", stall.MapLink);
        Assert.Equal(string.Empty, stall.AudioUrl);
        Assert.True(stall.IsActive);
        Assert.Contains("Chào mừng bạn đến với quán Ốc Vĩnh Khánh", stall.NarrationScriptVi);
        Assert.Contains("\"author\":\"Minh Anh\"", stall.ReviewsJson);
    }

    [Fact]
    public async Task GetStallById_ReturnsNotFound_WhenMissing()
    {
        var response = await _client.GetAsync("/api/stalls/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetNearbyStalls_ReturnsMatchesSortedByNearestFirst()
    {
        var stalls = await _client.GetFromJsonAsync<List<NearbyStallDto>>("/api/stalls/nearby?lat=10.7593&lng=106.7046&radius=250");

        Assert.NotNull(stalls);
        Assert.NotEmpty(stalls);
        Assert.Equal(1, stalls[0].Id);
        Assert.All(stalls, stall => Assert.InRange(stall.DistanceMeters, 0, 250));
        Assert.All(stalls, stall =>
        {
            Assert.True(stall.IsActive);
            Assert.False(string.IsNullOrWhiteSpace(stall.MapLink));
            Assert.False(string.IsNullOrWhiteSpace(stall.NarrationScriptVi));
            Assert.False(string.IsNullOrWhiteSpace(stall.Address));
        });

        for (var index = 1; index < stalls.Count; index++)
        {
            Assert.True(stalls[index - 1].DistanceMeters <= stalls[index].DistanceMeters);
        }
    }

    [Fact]
    public async Task GetTranslation_ReturnsVietnameseSource_WhenLangIsVi()
    {
        _factory.ResetTranslationState();

        var translation = await _client.GetFromJsonAsync<StallTranslationDto>("/api/stalls/1/translation?lang=vi");

        Assert.NotNull(translation);
        Assert.Equal("vi", translation.LanguageCode);
        Assert.Equal("Ốc Vĩnh Khánh", translation.Name);
        Assert.Contains("Chào mừng bạn đến với quán Ốc Vĩnh Khánh", translation.Description);
        Assert.Equal(0, GetFakeTranslator().TranslationCallCount);
    }

    [Fact]
    public async Task GetTranslation_ReturnsStoredEnglishTranslation()
    {
        _factory.ResetTranslationState();

        var translation = await _client.GetFromJsonAsync<StallTranslationDto>("/api/stalls/1/translation?lang=en");

        Assert.NotNull(translation);
        Assert.Equal("en", translation.LanguageCode);
        Assert.Equal("Ốc Vĩnh Khánh", translation.Name);
        Assert.Contains("Welcome to Oc Vinh Khanh", translation.Description);
        Assert.Equal(0, GetFakeTranslator().TranslationCallCount);
    }

    [Fact]
    public async Task GetTranslation_ReturnsStoredGermanTranslation()
    {
        _factory.ResetTranslationState();

        var translation = await _client.GetFromJsonAsync<StallTranslationDto>("/api/stalls/1/translation?lang=de");

        Assert.NotNull(translation);
        Assert.Equal("de", translation!.LanguageCode);
        Assert.Contains("Willkommen", translation.Description);
    }

    [Theory]
    [InlineData("ko")]
    [InlineData("ja")]
    public async Task GetTranslation_RejectsUnsupportedLanguages(string languageCode)
    {
        var response = await _client.GetAsync($"/api/stalls/1/translation?lang={languageCode}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private FakeTranslationService GetFakeTranslator()
    {
        return _factory.Services.GetRequiredService<FakeTranslationService>();
    }
}
