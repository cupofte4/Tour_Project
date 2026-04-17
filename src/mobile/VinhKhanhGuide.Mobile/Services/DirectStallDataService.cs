using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class DirectStallDataService : IStallDataService
{
    private readonly IStallApiClient _stallApiClient;

    public DirectStallDataService(IStallApiClient stallApiClient)
    {
        _stallApiClient = stallApiClient;
    }

    public Task<IReadOnlyList<StallSummary>> GetCachedStallsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<StallSummary>>([]);
    }

    public Task<IReadOnlyList<StallSummary>> RefreshStallsAsync(CancellationToken cancellationToken = default)
    {
        return _stallApiClient.GetStallsAsync(cancellationToken);
    }

    public Task<StallDetail?> GetCachedStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<StallDetail?>(null);
    }

    public Task<StallDetail?> RefreshStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
    {
        return _stallApiClient.GetStallByIdAsync(stallId, cancellationToken);
    }

    public Task UpdateLocalAudioPathAsync(int stallId, string? localAudioPath, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<StallTranslation?> GetCachedTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<StallTranslation?>(null);
    }

    public Task<StallTranslation?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        return _stallApiClient.GetStallTranslationAsync(stallId, languageCode, cancellationToken);
    }
}
