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
}
