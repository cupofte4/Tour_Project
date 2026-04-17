using System.Net;
using System.Net.Http;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.Services;

namespace VinhKhanhGuide.Mobile.Tests;

public class OfflineAudioDownloadServiceTests
{
    [Fact]
    public void GetDeterministicLocalPath_UsesStableFolderAndExtension()
    {
        var appDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new OfflineAudioDownloadService(new HttpClient(new FakeHttpMessageHandler()), appDataDirectory);

        var detail = new StallDetail
        {
            Id = 42,
            AudioUrl = "https://example.com/media/audio-track.m4a"
        };

        var firstPath = service.GetDeterministicLocalPath(detail);
        var secondPath = service.GetDeterministicLocalPath(detail);

        Assert.Equal(firstPath, secondPath);
        Assert.Equal(Path.Combine(appDataDirectory, "offline-audio", "stall-42.m4a"), firstPath);
    }

    [Fact]
    public async Task DownloadAsync_SavesFileToDeterministicPath()
    {
        var appDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(appDataDirectory);
        var service = new OfflineAudioDownloadService(
            new HttpClient(new FakeHttpMessageHandler([1, 2, 3, 4])),
            appDataDirectory);
        var detail = new StallDetail
        {
            Id = 7,
            AudioUrl = "https://example.com/media/audio.mp3"
        };

        try
        {
            var result = await service.DownloadAsync(detail);

            Assert.True(result.Succeeded);
            Assert.True(File.Exists(result.LocalAudioPath));
            Assert.Equal([1, 2, 3, 4], await File.ReadAllBytesAsync(result.LocalAudioPath));
        }
        finally
        {
            if (Directory.Exists(appDataDirectory))
            {
                Directory.Delete(appDataDirectory, recursive: true);
            }
        }
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly byte[] _payload;

        public FakeHttpMessageHandler(byte[]? payload = null)
        {
            _payload = payload ?? [1, 2, 3];
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_payload)
            });
        }
    }
}
