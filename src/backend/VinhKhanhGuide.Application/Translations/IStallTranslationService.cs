namespace VinhKhanhGuide.Application.Translations;

public interface IStallTranslationService
{
    Task<StallTranslationDto?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default);
}
