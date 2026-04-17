using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IExternalEntryService
{
    Task<ExternalEntryProcessResult> ProcessAsync(
        string? rawPayload,
        ExternalEntrySourceType sourceType,
        CancellationToken cancellationToken = default);
}
