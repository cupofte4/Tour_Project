using System.Net;
using System.Net.Http.Json;
using VinhKhanhGuide.Application.Stalls;

namespace VinhKhanhGuide.Api.Tests;

public class StallEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StallEndpointsTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStalls_ReturnsSeededStalls()
    {
        var stalls = await _client.GetFromJsonAsync<List<StallDto>>("/api/stalls");

        Assert.NotNull(stalls);
        Assert.NotEmpty(stalls);
        Assert.Contains(stalls, stall => stall.Id == 1 && stall.Name == "Oc Co Lan");
    }

    [Fact]
    public async Task GetStallById_ReturnsMatchingStall()
    {
        var stall = await _client.GetFromJsonAsync<StallDto>("/api/stalls/1");

        Assert.NotNull(stall);
        Assert.Equal(1, stall.Id);
        Assert.Equal("Oc Co Lan", stall.Name);
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
        var stalls = await _client.GetFromJsonAsync<List<NearbyStallDto>>("/api/stalls/nearby?lat=10.76042&lng=106.69321&radius=120");

        Assert.NotNull(stalls);
        Assert.NotEmpty(stalls);
        Assert.Equal(1, stalls[0].Id);
        Assert.All(stalls, stall => Assert.InRange(stall.DistanceMeters, 0, 120));

        for (var index = 1; index < stalls.Count; index++)
        {
            Assert.True(stalls[index - 1].DistanceMeters <= stalls[index].DistanceMeters);
        }
    }
}
