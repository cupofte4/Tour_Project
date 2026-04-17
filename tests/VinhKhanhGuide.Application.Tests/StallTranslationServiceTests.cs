using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Application.Tests;

public class StallTranslationServiceTests
{
    [Fact]
    public async Task GetTranslationAsync_ReturnsStoredGermanTranslation_WhenAvailable()
    {
        var stallRepository = new FakeStallReadRepository();
        var fakeTranslationService = new FakeTranslationService();
        var translationRepository = new FakeTranslationRepository(
            new StallTranslationDto
            {
                StallId = 1,
                LanguageCode = "de",
                Name = "Deutscher Name",
                Description = "Deutsche Beschreibung"
            },
            new StallTranslationDto
            {
                StallId = 1,
                LanguageCode = "en",
                Name = "English name",
                Description = "English description"
            });
        var service = new StallTranslationService(stallRepository, translationRepository, fakeTranslationService);

        var result = await service.GetTranslationAsync(1, "de");

        Assert.NotNull(result);
        Assert.Equal("de", result!.RequestedLanguageCode);
        Assert.Equal("de", result.LanguageCode);
        Assert.Equal("Deutsche Beschreibung", result.Description);
        Assert.False(result.UsedFallback);
        Assert.Equal("stored", result.Source);
        Assert.Equal(0, fakeTranslationService.CallCount);
    }

    [Fact]
    public async Task GetTranslationAsync_ReturnsNull_WhenRequestedTranslationIsUnavailable()
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

        var result = await service.GetTranslationAsync(1, "zh");

        Assert.Null(result);
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

    [Fact]
    public async Task GetTranslationAsync_ReturnsVietnameseSource_WhenRequestedLanguageIsVietnamese()
    {
        var stallRepository = new FakeStallReadRepository();
        var translationRepository = new FakeTranslationRepository();
        var service = new StallTranslationService(stallRepository, translationRepository, new FakeTranslationService());

        var result = await service.GetTranslationAsync(1, "vi");

        Assert.NotNull(result);
        Assert.Equal("vi", result!.RequestedLanguageCode);
        Assert.Equal("vi", result.LanguageCode);
        Assert.False(result.UsedFallback);
        Assert.Equal("vietnamese-source", result.Source);
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
        public int CallCount { get; private set; }

        public Task<string> TranslateAsync(string text, string targetLanguageCode, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(text);
        }
    }
}
