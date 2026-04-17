using Microsoft.Maui.Media;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class TtsLocaleDiscoveryServiceTests
{
    [Fact]
    public async Task GetAvailableLocalesAsync_UsesMauiFallbackWhenIosDiscoveryFails_AndCachesSuccess()
    {
        var iosCallCount = 0;
        var mauiCallCount = 0;
        var service = new TtsLocaleDiscoveryService(
            _ =>
            {
                mauiCallCount++;
                return Task.FromResult<IReadOnlyList<Locale>>([TextToSpeechSettingsResolver.CreateLocale("vi-VN")]);
            },
            () =>
            {
                iosCallCount++;
                throw new InvalidOperationException("corrupted voice metadata");
            });

        var first = await service.GetAvailableLocalesAsync();
        var second = await service.GetAvailableLocalesAsync();

        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal("vi-VN", first[0].Name);
        Assert.Equal(1, iosCallCount);
        Assert.Equal(1, mauiCallCount);
    }

    [Fact]
    public async Task GetAvailableLocalesAsync_ThrowsDiscoveryException_WhenAllDiscoveryPathsFail()
    {
        var service = new TtsLocaleDiscoveryService(
            _ => throw new InvalidOperationException("maui failed"),
            () => throw new InvalidOperationException("ios failed"));

        var exception = await Assert.ThrowsAsync<TextToSpeechLocaleDiscoveryException>(() => service.GetAvailableLocalesAsync());

        Assert.Equal("Text-to-speech voices couldn't be discovered on this device.", exception.Message);
    }
}
