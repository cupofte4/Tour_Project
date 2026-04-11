using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class TextToSpeechSettingsResolverTests
{
    [Fact]
    public void ResolvePreferredLocaleCode_ReturnsExactMatch()
    {
        var localeCode = TextToSpeechSettingsResolver.ResolvePreferredLocaleCode("en-US", CreateLocaleCodes());

        Assert.Equal("en-US", localeCode);
    }

    [Fact]
    public void ResolvePreferredLocaleCode_FallsBackToLanguagePrefix()
    {
        var localeCode = TextToSpeechSettingsResolver.ResolvePreferredLocaleCode("en-AU", CreateLocaleCodes());

        Assert.Equal("en-US", localeCode);
    }

    [Fact]
    public void ResolvePreferredLocaleCode_ReturnsNull_WhenUnsupported()
    {
        var localeCode = TextToSpeechSettingsResolver.ResolvePreferredLocaleCode("fr-FR", CreateLocaleCodes());

        Assert.Null(localeCode);
    }

    [Theory]
    [InlineData("vi-VN", "vi")]
    [InlineData("en-US", "en")]
    [InlineData("zh-Hans", "zh")]
    [InlineData("de-DE", "de")]
    public void ResolveSupportedLanguageCode_MapsSupportedDeviceLanguages(string deviceLanguage, string expectedLanguage)
    {
        var resolved = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(deviceLanguage);

        Assert.Equal(expectedLanguage, resolved);
    }

    [Theory]
    [InlineData("fr-FR", "en")]
    [InlineData("", "en")]
    [InlineData(null, "en")]
    public void ResolveSupportedLanguageCode_FallsBackToEnglishThenVietnamese(string? deviceLanguage, string expectedLanguage)
    {
        var resolved = TextToSpeechSettingsResolver.ResolveSupportedLanguageCode(deviceLanguage);

        Assert.Equal(expectedLanguage, resolved);
    }

    [Fact]
    public void BuildLanguageFallbackChain_ReturnsRequestedThenEnglishThenVietnamese()
    {
        var chain = TextToSpeechSettingsResolver.BuildLanguageFallbackChain("de-DE");

        Assert.Equal(["de", "en", "vi"], chain);
    }

    private static IReadOnlyList<string> CreateLocaleCodes()
    {
        return ["en-US", "vi-VN", "de-DE"];
    }
}
