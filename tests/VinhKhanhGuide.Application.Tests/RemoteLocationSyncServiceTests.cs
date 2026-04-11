using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Application.Tests;

public class RemoteLocationSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_UpsertsDistinctLocationIds_AndPersistsTranslations()
    {
        var source = new FakeRemoteLocationContentSource(
        [
            new RemoteLocationRecord
            {
                Id = 1,
                Name = "Old name",
                Description = "Old description",
                Latitude = 10.759,
                Longitude = 106.704,
                TextVi = "Old vi"
            },
            new RemoteLocationRecord
            {
                Id = 1,
                Name = "New name",
                Description = "New description",
                Latitude = 10.760,
                Longitude = 106.705,
                TextVi = "New vi",
                TextEn = "English text"
            }
        ]);
        var stallRepository = new FakeStallContentSyncRepository();
        var translationRepository = new FakeStallTranslationRepository();
        var service = new RemoteLocationSyncService(source, stallRepository, translationRepository);

        var result = await service.SyncAsync();

        var syncedStall = Assert.Single(stallRepository.UpsertedStalls);
        Assert.Equal("New name", syncedStall.Name);
        Assert.Equal(1, result.ImportedStallCount);
        Assert.Single(result.SyncedIds);
        Assert.Single(translationRepository.SavedTranslations);
    }

    private sealed class FakeRemoteLocationContentSource(IReadOnlyList<RemoteLocationRecord> records) : IRemoteLocationContentSource
    {
        public Task<IReadOnlyList<RemoteLocationRecord>> GetLocationsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(records);
        }
    }

    private sealed class FakeStallContentSyncRepository : IStallContentSyncRepository
    {
        public List<StallDto> UpsertedStalls { get; } = [];

        public Task UpsertAsync(IReadOnlyCollection<StallDto> stalls, CancellationToken cancellationToken = default)
        {
            UpsertedStalls.AddRange(stalls);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStallTranslationRepository : IStallTranslationRepository
    {
        public List<StallTranslationDto> SavedTranslations { get; } = [];

        public Task<IReadOnlyList<StallTranslationDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallTranslationDto>>(SavedTranslations);
        }

        public Task<StallTranslationDto?> GetByStallIdAndLanguageAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StallTranslationDto?>(null);
        }

        public Task SaveAsync(StallTranslationDto translation, CancellationToken cancellationToken = default)
        {
            SavedTranslations.Add(translation);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            SavedTranslations.RemoveAll(item =>
                item.StallId == stallId &&
                string.Equals(item.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            return Task.CompletedTask;
        }
    }
}
