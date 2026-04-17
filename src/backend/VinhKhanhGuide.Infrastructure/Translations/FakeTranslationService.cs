using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Infrastructure.Translations;

public class FakeTranslationService : ITranslationService
{
    public int TranslationCallCount { get; private set; }

    public Task<string> TranslateAsync(string text, string targetLanguageCode, CancellationToken cancellationToken = default)
    {
        TranslationCallCount++;
        return Task.FromResult($"[{targetLanguageCode.ToUpperInvariant()}] {text}");
    }

    public void Reset()
    {
        TranslationCallCount = 0;
    }
}
