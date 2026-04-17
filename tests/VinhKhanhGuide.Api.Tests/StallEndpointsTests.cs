using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Infrastructure.Persistence;
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
        _factory.ResetData();
        _factory.ResetTranslationState();

        var translation = await _client.GetFromJsonAsync<StallTranslationDto>("/api/stalls/1/translation?lang=vi");

        Assert.NotNull(translation);
        Assert.Equal("vi", translation.RequestedLanguageCode);
        Assert.Equal("vi", translation.LanguageCode);
        Assert.Equal("Ốc Vĩnh Khánh", translation.Name);
        Assert.Contains("Chào mừng bạn đến với quán Ốc Vĩnh Khánh", translation.Description);
        Assert.False(translation.UsedFallback);
        Assert.Equal("vietnamese-source", translation.Source);
        Assert.Equal(0, GetFakeTranslator().TranslationCallCount);
    }

    [Fact]
    public async Task GetTranslation_ReturnsStoredEnglishTranslation()
    {
        _factory.ResetData();
        _factory.ResetTranslationState();

        var translation = await _client.GetFromJsonAsync<StallTranslationDto>("/api/stalls/1/translation?lang=en");

        Assert.NotNull(translation);
        Assert.Equal("en", translation.RequestedLanguageCode);
        Assert.Equal("en", translation.LanguageCode);
        Assert.Equal("Ốc Vĩnh Khánh", translation.Name);
        Assert.Contains("Welcome to Oc Vinh Khanh", translation.Description);
        Assert.False(translation.UsedFallback);
        Assert.Equal("stored", translation.Source);
        Assert.Equal(0, GetFakeTranslator().TranslationCallCount);
    }

    [Fact]
    public async Task GetTranslation_ReturnsStoredGermanTranslation()
    {
        _factory.ResetData();
        _factory.ResetTranslationState();

        var translation = await _client.GetFromJsonAsync<StallTranslationDto>("/api/stalls/1/translation?lang=de");

        Assert.NotNull(translation);
        Assert.Equal("de", translation!.RequestedLanguageCode);
        Assert.Equal("de", translation!.LanguageCode);
        Assert.Contains("Willkommen", translation.Description);
        Assert.False(translation.UsedFallback);
        Assert.Equal("stored", translation.Source);
    }

    [Fact]
    public async Task GetTranslation_ReturnsStoredChineseTranslation()
    {
        _factory.ResetData();
        _factory.ResetTranslationState();

        var translation = await _client.GetFromJsonAsync<StallTranslationDto>("/api/stalls/1/translation?lang=zh");

        Assert.NotNull(translation);
        Assert.Equal("zh", translation!.RequestedLanguageCode);
        Assert.Equal("zh", translation.LanguageCode);
        Assert.False(string.IsNullOrWhiteSpace(translation.Description));
        Assert.False(translation.UsedFallback);
        Assert.Equal("stored", translation.Source);
    }

    [Fact]
    public async Task GetTranslation_ReturnsNotFound_WhenRequestedTranslationIsMissing()
    {
        _factory.ResetData();
        _factory.ResetTranslationState();
        await RemoveStoredTranslationAsync(1, "de");

        var response = await _client.GetAsync("/api/stalls/1/translation?lang=de");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, GetFakeTranslator().TranslationCallCount);
    }

    [Theory]
    [InlineData("ko")]
    [InlineData("ja")]
    public async Task GetTranslation_RejectsUnsupportedLanguages(string languageCode)
    {
        var response = await _client.GetAsync($"/api/stalls/1/translation?lang={languageCode}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SyncLocations_ReturnsStableSummary_AndIsIdempotent()
    {
        var snapshotPath = CreateSnapshotFile(
            """
            INSERT INTO Locations (Id, Name, Description, Image, Images, Address, Phone, ReviewsJson, Latitude, Longitude, TextVi, TextEn, TextZh, TextDe)
            VALUES
            (101, 'Stall One', 'Desc 1', '', '[]', 'Address 1', '0901', '[]', 10.7590, 106.7040, 'Vi 1', 'En 1', 'Zh 1', 'De 1'),
            (102, 'Stall Two', 'Desc 2', '', '[]', 'Address 2', '0902', '[]', 10.7600, 106.7050, 'Vi 2', 'En 2', 'Zh 2', 'De 2');
            """);

        _factory.ResetData(snapshotPath);

        var first = await _client.PostAsync("/api/stalls/sync/locations", content: null);
        var firstResult = await first.Content.ReadFromJsonAsync<RemoteLocationSyncResult>();
        var second = await _client.PostAsync("/api/stalls/sync/locations", content: null);
        var secondResult = await second.Content.ReadFromJsonAsync<RemoteLocationSyncResult>();

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        Assert.Equal(2, firstResult!.DistinctSourceRecordCount);
        Assert.Equal(0, firstResult.InsertedStallCount);
        Assert.Equal(0, firstResult.UpdatedStallCount);
        Assert.Equal(2, firstResult.UnchangedStallCount);
        Assert.Equal(0, firstResult.InsertedTranslationCount);
        Assert.Equal(6, firstResult.UnchangedTranslationCount);
        Assert.Equal(0, secondResult!.InsertedStallCount);
        Assert.Equal(0, secondResult.UpdatedStallCount);
        Assert.Equal(2, secondResult.UnchangedStallCount);
        Assert.Equal(0, secondResult.InsertedTranslationCount);
        Assert.Equal(6, secondResult.UnchangedTranslationCount);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(2, dbContext.Stalls.Count(stall => stall.IsActive));
        Assert.Equal(6, dbContext.StallTranslations.Count());
    }

    [Fact]
    public async Task SyncLocations_UpdatesAndDeactivates_WhenSnapshotChanges()
    {
        var initialSnapshotPath = CreateSnapshotFile(
            """
            INSERT INTO Locations (Id, Name, Description, Image, Images, Address, Phone, ReviewsJson, Latitude, Longitude, TextVi, TextEn, TextZh, TextDe)
            VALUES
            (201, 'Initial One', 'Desc 1', '', '[]', 'Address 1', '0901', '[]', 10.7590, 106.7040, 'Vi 1', 'En 1', 'Zh 1', 'De 1'),
            (202, 'Initial Two', 'Desc 2', '', '[]', 'Address 2', '0902', '[]', 10.7600, 106.7050, 'Vi 2', 'En 2', 'Zh 2', 'De 2');
            """);
        var changedSnapshotPath = CreateSnapshotFile(
            """
            INSERT INTO Locations (Id, Name, Description, Image, Images, Address, Phone, ReviewsJson, Latitude, Longitude, TextVi, TextEn, TextZh, TextDe)
            VALUES
            (201, 'Updated One', 'Desc 1', '', '[]', 'Address 1', '0901', '[]', 10.7590, 106.7040, 'Vi 1 updated', 'En 1 updated', '', 'De 1');
            """);

        _factory.ResetData(initialSnapshotPath);
        _factory.SetSnapshotPath(changedSnapshotPath);

        var response = await _client.PostAsync("/api/stalls/sync/locations", content: null);
        var result = await response.Content.ReadFromJsonAsync<RemoteLocationSyncResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result!.DistinctSourceRecordCount);
        Assert.Equal(1, result.UpdatedStallCount);
        Assert.Equal(1, result.DeactivatedStallCount);
        Assert.Equal(4, result.RemovedTranslationCount);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var updatedStall = dbContext.Stalls.Single(stall => stall.Id == 201);
        var deactivatedStall = dbContext.Stalls.Single(stall => stall.Id == 202);

        Assert.Equal("Updated One", updatedStall.Name);
        Assert.True(updatedStall.IsActive);
        Assert.False(deactivatedStall.IsActive);
        Assert.DoesNotContain(dbContext.StallTranslations, translation => translation.StallId == 201 && translation.LanguageCode == "zh");
        Assert.Contains(dbContext.StallTranslations, translation => translation.StallId == 201 && translation.LanguageCode == "en" && translation.Description == "En 1 updated");
    }

    private FakeTranslationService GetFakeTranslator()
    {
        return _factory.Services.GetRequiredService<FakeTranslationService>();
    }

    private static string CreateSnapshotFile(string sql)
    {
        var path = Path.Combine(Path.GetTempPath(), $"foodguide-test-{Guid.NewGuid():N}.sql");
        File.WriteAllText(path, sql);
        return path;
    }

    private async Task RemoveStoredTranslationAsync(int stallId, string languageCode)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var translation = dbContext.StallTranslations.Single(item =>
            item.StallId == stallId &&
            item.LanguageCode == languageCode);

        dbContext.StallTranslations.Remove(translation);
        await dbContext.SaveChangesAsync();
    }
}
