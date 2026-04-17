using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class CachedStallDataService : IStallDataService
{
    private readonly IStallApiClient _stallApiClient;
    private readonly IStallDataCache _stallDataCache;

    public CachedStallDataService(IStallApiClient stallApiClient, IStallDataCache stallDataCache)
    {
        _stallApiClient = stallApiClient;
        _stallDataCache = stallDataCache;
    }

    public Task<IReadOnlyList<StallSummary>> GetCachedStallsAsync(CancellationToken cancellationToken = default)
    {
        return _stallDataCache.GetStallsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StallSummary>> RefreshStallsAsync(CancellationToken cancellationToken = default)
    {
        var stalls = await _stallApiClient.GetStallsAsync(cancellationToken);
        await _stallDataCache.SaveStallsAsync(stalls, cancellationToken);
        return stalls;
    }

    public Task<StallDetail?> GetCachedStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
    {
        return _stallDataCache.GetStallDetailAsync(stallId, cancellationToken);
    }

    public async Task<StallDetail?> RefreshStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
    {
        var stallDetail = await _stallApiClient.GetStallByIdAsync(stallId, cancellationToken);

        if (stallDetail is not null)
        {
            await _stallDataCache.SaveStallDetailAsync(stallDetail, cancellationToken);
        }

        return stallDetail;
    }

    public Task UpdateLocalAudioPathAsync(int stallId, string? localAudioPath, CancellationToken cancellationToken = default)
    {
        return _stallDataCache.UpdateLocalAudioPathAsync(stallId, localAudioPath, cancellationToken);
    }

    public Task<StallTranslation?> GetCachedTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        return _stallDataCache.GetTranslationAsync(stallId, languageCode, cancellationToken);
    }

    public async Task<StallTranslation?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        var cachedTranslation = await _stallDataCache.GetTranslationAsync(stallId, languageCode, cancellationToken);
        if (cachedTranslation is not null)
        {
            return cachedTranslation;
        }

        var translation = await _stallApiClient.GetStallTranslationAsync(stallId, languageCode, cancellationToken);

        if (translation is not null)
        {
            await _stallDataCache.SaveTranslationAsync(translation, cancellationToken);
        }

        return translation;
    }
}
