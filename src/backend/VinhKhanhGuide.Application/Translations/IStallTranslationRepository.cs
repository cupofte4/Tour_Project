namespace VinhKhanhGuide.Application.Translations;

public interface IStallTranslationRepository
{
    Task<IReadOnlyList<StallTranslationDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<StallTranslationDto?> GetByStallIdAndLanguageAsync(int stallId, string languageCode, CancellationToken cancellationToken = default);

    Task SaveAsync(StallTranslationDto translation, CancellationToken cancellationToken = default);

    Task DeleteAsync(int stallId, string languageCode, CancellationToken cancellationToken = default);
}
