namespace VinhKhanhGuide.Application.Translations;

public interface ITranslationService
{
    Task<string> TranslateAsync(string text, string targetLanguageCode, CancellationToken cancellationToken = default);
}
