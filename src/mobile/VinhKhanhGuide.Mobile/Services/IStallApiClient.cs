using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IStallApiClient
{
    Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default);

    Task<StallDetail?> GetStallByIdAsync(int stallId, CancellationToken cancellationToken = default);

    Task<StallTranslation?> GetStallTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default);
}
