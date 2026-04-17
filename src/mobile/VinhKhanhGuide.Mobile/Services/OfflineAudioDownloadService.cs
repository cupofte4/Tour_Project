using System.Net.Http;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class OfflineAudioDownloadService : IOfflineAudioDownloadService
{
    private const string OfflineAudioFolderName = "offline-audio";
    private readonly HttpClient _httpClient;
    private readonly string _appDataDirectory;

    public OfflineAudioDownloadService(HttpClient httpClient)
        : this(httpClient, FileSystem.AppDataDirectory)
    {
    }

    public OfflineAudioDownloadService(HttpClient httpClient, string appDataDirectory)
    {
        _httpClient = httpClient;
        _appDataDirectory = appDataDirectory;
    }

    public bool CanDownloadAudio(StallDetail? stallDetail)
    {
        return stallDetail is not null && Uri.TryCreate(stallDetail.AudioUrl, UriKind.Absolute, out _);
    }

    public string GetDeterministicLocalPath(StallDetail stallDetail)
    {
        var extension = ResolveFileExtension(stallDetail.AudioUrl);
        return Path.Combine(GetOfflineAudioDirectory(), $"stall-{stallDetail.Id}{extension}");
    }

    public async Task<OfflineAudioDownloadResult> DownloadAsync(StallDetail stallDetail, CancellationToken cancellationToken = default)
    {
        if (!CanDownloadAudio(stallDetail))
        {
            return OfflineAudioDownloadResult.Failure("Offline audio is not available for this location.");
        }

        Directory.CreateDirectory(GetOfflineAudioDirectory());

        var destinationPath = GetDeterministicLocalPath(stallDetail);
        var temporaryPath = $"{destinationPath}.download";

        try
        {
            using var response = await _httpClient.GetAsync(
                stallDetail.AudioUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destinationStream = File.Create(temporaryPath);
            await sourceStream.CopyToAsync(destinationStream, cancellationToken);
            await destinationStream.FlushAsync(cancellationToken);

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            File.Move(temporaryPath, destinationPath);
            return OfflineAudioDownloadResult.Success(destinationPath);
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            return OfflineAudioDownloadResult.Failure("We couldn't download offline audio right now.");
        }
    }

    public Task<bool> RemoveAsync(string? localAudioPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(localAudioPath))
        {
            return Task.FromResult(false);
        }

        var trimmedPath = localAudioPath.Trim();
        if (!File.Exists(trimmedPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(trimmedPath);
        return Task.FromResult(true);
    }

    private string GetOfflineAudioDirectory()
    {
        return Path.Combine(_appDataDirectory, OfflineAudioFolderName);
    }

    private static string ResolveFileExtension(string? audioUrl)
    {
        if (!Uri.TryCreate(audioUrl, UriKind.Absolute, out var audioUri))
        {
            return ".audio";
        }

        var extension = Path.GetExtension(audioUri.AbsolutePath);
        if (string.IsNullOrWhiteSpace(extension) || extension.Length > 10)
        {
            return ".audio";
        }

        return extension;
    }
}
