using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class ExternalStallRouteParserTests
{
    private readonly ExternalStallRouteParser _parser = new();

    [Fact]
    public void Parse_StallShortCode_ReturnsValidRoute()
    {
        var route = _parser.Parse("stall:1", ExternalEntrySourceType.Qr);

        Assert.True(route.IsValid);
        Assert.Equal(1, route.StallId);
        Assert.False(route.AutoPlay);
        Assert.Null(route.RequestedLanguageCode);
        Assert.Equal(ExternalEntrySourceType.Qr, route.SourceType);
    }

    [Fact]
    public void Parse_PoiShortCode_NormalizesToValidRoute()
    {
        var route = _parser.Parse("poi:1", ExternalEntrySourceType.Manual);

        Assert.True(route.IsValid);
        Assert.Equal(1, route.StallId);
        Assert.Equal(ExternalEntrySourceType.Manual, route.SourceType);
    }

    [Fact]
    public void Parse_CustomSchemeRoute_ReturnsValidRoute()
    {
        var route = _parser.Parse("vinhkhanhguide://stall/5", ExternalEntrySourceType.DeepLink);

        Assert.True(route.IsValid);
        Assert.Equal(5, route.StallId);
        Assert.False(route.AutoPlay);
        Assert.Null(route.RequestedLanguageCode);
    }

    [Fact]
    public void Parse_LegacyCustomSchemeRoute_RemainsSupported()
    {
        var route = _parser.Parse("vkguide://stall/5", ExternalEntrySourceType.DeepLink);

        Assert.True(route.IsValid);
        Assert.Equal(5, route.StallId);
    }

    [Fact]
    public void Parse_QueryParameters_ReadsAutoPlayAndLanguage()
    {
        var route = _parser.Parse(
            "vinhkhanhguide://stall/5?autoplay=1&lang=en&ignored=yes",
            ExternalEntrySourceType.DeepLink);

        Assert.True(route.IsValid);
        Assert.Equal(5, route.StallId);
        Assert.True(route.AutoPlay);
        Assert.Equal("en", route.RequestedLanguageCode);
    }

    [Fact]
    public void Parse_UnsupportedLanguage_IgnoresLanguageSafely()
    {
        var route = _parser.Parse(
            "vinhkhanhguide://stall/5?autoplay=1&lang=fr",
            ExternalEntrySourceType.DeepLink);

        Assert.True(route.IsValid);
        Assert.Equal(5, route.StallId);
        Assert.True(route.AutoPlay);
        Assert.Null(route.RequestedLanguageCode);
    }

    [Fact]
    public void Parse_OpenRouteWithQueryString_ReturnsValidRoute()
    {
        var route = _parser.Parse(
            "vinhkhanhguide://open?stallId=9&autoplay=true&lang=de",
            ExternalEntrySourceType.DeepLink);

        Assert.True(route.IsValid);
        Assert.Equal(9, route.StallId);
        Assert.True(route.AutoPlay);
        Assert.Equal("de", route.RequestedLanguageCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("stall:")]
    [InlineData("stall:abc")]
    [InlineData("vinhkhanhguide://stall")]
    [InlineData("vinhkhanhguide://stall/abc")]
    [InlineData("vinhkhanhguide://map/1")]
    [InlineData("vinhkhanhguide://open")]
    [InlineData("not-a-route")]
    public void Parse_MalformedPayloads_ReturnInvalidRoute(string payload)
    {
        var route = _parser.Parse(payload, ExternalEntrySourceType.Manual);

        Assert.False(route.IsValid);
        Assert.NotEmpty(route.ErrorMessage);
    }
}
