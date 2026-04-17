using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IStallDataCache
{
    Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default);

    Task SaveStallsAsync(IReadOnlyList<StallSummary> stalls, CancellationToken cancellationToken = default);

    Task<StallDetail?> GetStallDetailAsync(int stallId, CancellationToken cancellationToken = default);

    Task SaveStallDetailAsync(StallDetail stallDetail, CancellationToken cancellationToken = default);

    Task UpdateLocalAudioPathAsync(int stallId, string? localAudioPath, CancellationToken cancellationToken = default);

    Task<StallTranslation?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default);

    Task SaveTranslationAsync(StallTranslation translation, CancellationToken cancellationToken = default);
}
