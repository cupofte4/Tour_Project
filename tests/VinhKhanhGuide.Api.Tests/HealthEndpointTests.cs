using Microsoft.AspNetCore.Mvc.Testing;

namespace VinhKhanhGuide.Api.Tests;

public class HealthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Root_ReturnsSuccessStatusCode()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DemoOpenRoute_ReturnsHtmlLandingPage()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/demo/open");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        Assert.Contains("Vinh Khanh Guide Demo", content);
        Assert.Contains("href=\"vinhkhanhguide://open\"", content);
    }

    [Fact]
    public async Task DemoOpenStallRoute_EmbedsSelectedStallLink()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/demo/open/stall/12");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("Selected stall: 12", content);
        Assert.Contains("href=\"vinhkhanhguide://stall/12\"", content);
    }
}
