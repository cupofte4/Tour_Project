using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Application.Tests;

public class StallTranslationServiceTests
{
    [Fact]
    public async Task GetTranslationAsync_FallsBackToEnglish_WhenGermanTranslationIsMissing()
    {
        var stallRepository = new FakeStallReadRepository();
        var translationRepository = new FakeTranslationRepository(new StallTranslationDto
        {
            StallId = 1,
            LanguageCode = "en",
            Name = "English name",
            Description = "English description"
        });
        var service = new StallTranslationService(stallRepository, translationRepository, new FakeTranslationService());

        var result = await service.GetTranslationAsync(1, "de");

        Assert.NotNull(result);
        Assert.Equal("en", result!.LanguageCode);
        Assert.Equal("English description", result.Description);
    }

    [Fact]
    public async Task GetTranslationAsync_FallsBackToVietnamese_WhenEnglishIsUnavailable()
    {
        var stallRepository = new FakeStallReadRepository();
        var translationRepository = new FakeTranslationRepository();
        var service = new StallTranslationService(stallRepository, translationRepository, new FakeTranslationService());

        var result = await service.GetTranslationAsync(1, "zh");

        Assert.NotNull(result);
        Assert.Equal("vi", result!.LanguageCode);
        Assert.Equal("Noi dung Viet", result.Description);
    }

    [Fact]
    public async Task GetTranslationAsync_ReturnsNull_ForUnsupportedLanguage()
    {
        var stallRepository = new FakeStallReadRepository();
        var translationRepository = new FakeTranslationRepository();
        var service = new StallTranslationService(stallRepository, translationRepository, new FakeTranslationService());

        var result = await service.GetTranslationAsync(1, "ko");

        Assert.Null(result);
    }

    private sealed class FakeStallReadRepository : IStallReadRepository
    {
        public Task<IReadOnlyList<StallDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallDto>>([]);
        }

        public Task<StallDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallDto?>(new StallDto
            {
                Id = id,
                Name = "Ten Viet",
                DescriptionVi = "Mo ta ngan",
                NarrationScriptVi = "Noi dung Viet"
            });
        }
    }

    private sealed class FakeTranslationRepository(params StallTranslationDto[] translations) : IStallTranslationRepository
    {
        private readonly IReadOnlyList<StallTranslationDto> _translations = translations;

        public Task<IReadOnlyList<StallTranslationDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_translations);
        }

        public Task<StallTranslationDto?> GetByStallIdAndLanguageAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            var translation = _translations.FirstOrDefault(item =>
                item.StallId == stallId &&
                string.Equals(item.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(translation);
        }

        public Task SaveAsync(StallTranslationDto translation, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTranslationService : ITranslationService
    {
        public Task<string> TranslateAsync(string text, string targetLanguageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(text);
        }
    }
}
