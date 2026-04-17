using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class TextToSpeechSettingsResolverTests
{
    [Fact]
    public void ResolvePreferredLocaleCode_ReturnsExactMatch()
    {
        var localeCode = TextToSpeechSettingsResolver.ResolvePreferredLocaleCode("en", null, CreateLocaleCodes());

        Assert.Equal("en-US", localeCode);
    }

    [Theory]
    [InlineData("vi-VN", "vi", "VN", "vi-VN", "vi-VN")]
    [InlineData("zh-Hans", "zh", "", "zh-Hans", "zh-Hans")]
    [InlineData("de-DE", "de", "DE", "de-DE", "de-DE")]
    public void CreateLocale_BuildsDeterministicLocaleShape(
        string localeCode,
        string expectedLanguage,
        string expectedCountry,
        string expectedName,
        string expectedId)
    {
        var locale = TextToSpeechSettingsResolver.CreateLocale(localeCode);

        Assert.Equal(expectedLanguage, locale.Language);
        Assert.Equal(expectedCountry, locale.Country);
        Assert.Equal(expectedName, locale.Name);
        Assert.Equal(expectedId, locale.Id);
    }

    [Fact]
    public void ResolvePreferredLocaleCode_FallsBackToSameLanguageVariant()
    {
        var localeCode = TextToSpeechSettingsResolver.ResolvePreferredLocaleCode("zh", null, ["zh-HK"]);

        Assert.Equal("zh-HK", localeCode);
    }

    [Fact]
    public void ResolvePreferredLocaleCode_PrefersRequestedLocaleHint_WhenAvailable()
    {
        var localeCode = TextToSpeechSettingsResolver.ResolvePreferredLocaleCode("zh", "zh-Hans", ["zh-Hans", "zh-CN"]);

        Assert.Equal("zh-Hans", localeCode);
    }

    [Fact]
    public void ResolvePreferredLocaleCode_ReturnsNull_WhenNoSameLanguageLocaleExists()
    {
        var localeCode = TextToSpeechSettingsResolver.ResolvePreferredLocaleCode("zh", null, CreateLocaleCodes());

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
    public void BuildLanguageFallbackChain_ReturnsRequestedThenVietnameseOnly()
    {
        var chain = TextToSpeechSettingsResolver.BuildLanguageFallbackChain("de-DE");

        Assert.Equal(["de", "vi"], chain);
    }

    [Theory]
    [InlineData("vi", false)]
    [InlineData("en", true)]
    [InlineData("zh", true)]
    [InlineData("de", true)]
    public void ShouldPreferTextToSpeech_UsesTranslatedLanguages(string languageCode, bool expected)
    {
        var result = TextToSpeechSettingsResolver.ShouldPreferTextToSpeech(languageCode);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("vi", "Vietnamese")]
    [InlineData("en", "English")]
    [InlineData("zh", "Chinese")]
    [InlineData("de", "German")]
    public void GetLanguageDisplayName_ReturnsFriendlyName(string languageCode, string expected)
    {
        var result = TextToSpeechSettingsResolver.GetLanguageDisplayName(languageCode);

        Assert.Equal(expected, result);
    }

    private static IReadOnlyList<string> CreateLocaleCodes()
    {
        return ["en-US", "vi-VN", "de-DE"];
    }
}
