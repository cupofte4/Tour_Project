using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class StallNarrationResolverTests
{
    [Fact]
    public async Task ResolveAsync_UsesRequestedLanguageText_WhenAvailable()
    {
        var translations = new Dictionary<string, StallTranslation>(StringComparer.OrdinalIgnoreCase)
        {
            ["de"] = new()
            {
                StallId = 1,
                LanguageCode = "de",
                Name = "Deutscher Name",
                Description = "Deutscher Text"
            }
        };

        var resolved = await StallNarrationResolver.ResolveAsync(
            "de",
            "Ten Quan",
            "Mo ta tieng Viet",
            "https://example.com/audio.mp3",
            languageCode => Task.FromResult(translations.TryGetValue(languageCode, out var translation)
                ? translation
                : null));

        Assert.Equal("de", resolved.LanguageCode);
        Assert.Equal("Deutscher Text", resolved.Text);
        Assert.Equal(string.Empty, resolved.AudioUrl);
        Assert.True(resolved.CanNarrate);
    }

    [Fact]
    public async Task ResolveAsync_FallsBackToEnglish_BeforeVietnamese()
    {
        var translations = new Dictionary<string, StallTranslation>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new()
            {
                StallId = 2,
                LanguageCode = "en",
                Name = "English Name",
                Description = "English narration"
            }
        };

        var resolved = await StallNarrationResolver.ResolveAsync(
            "zh",
            "Ten Quan",
            "Mo ta tieng Viet",
            string.Empty,
            languageCode => Task.FromResult(translations.TryGetValue(languageCode, out var translation)
                ? translation
                : null));

        Assert.Equal("en", resolved.LanguageCode);
        Assert.Equal("English narration", resolved.Text);
    }

    [Fact]
    public async Task ResolveAsync_UsesVietnameseAudioFallback_WhenNoTextExists()
    {
        var resolved = await StallNarrationResolver.ResolveAsync(
            "de",
            "Ten Quan",
            string.Empty,
            "https://example.com/audio.mp3",
            _ => Task.FromResult<StallTranslation?>(null));

        Assert.Equal("vi", resolved.LanguageCode);
        Assert.Equal(string.Empty, resolved.Text);
        Assert.Equal("https://example.com/audio.mp3", resolved.AudioUrl);
        Assert.True(resolved.CanNarrate);
        Assert.False(resolved.HasText);
    }
}
