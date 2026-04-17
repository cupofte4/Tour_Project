using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IStallDataService
{
    Task<IReadOnlyList<StallSummary>> GetCachedStallsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StallSummary>> RefreshStallsAsync(CancellationToken cancellationToken = default);

    Task<StallDetail?> GetCachedStallDetailAsync(int stallId, CancellationToken cancellationToken = default);

    Task<StallDetail?> RefreshStallDetailAsync(int stallId, CancellationToken cancellationToken = default);

    Task UpdateLocalAudioPathAsync(int stallId, string? localAudioPath, CancellationToken cancellationToken = default);

    Task<StallTranslation?> GetCachedTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default);

    Task<StallTranslation?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default);
}
