using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Application.Tests;

public class RemoteLocationSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_FirstRun_InsertsStallsAndTranslations()
    {
        var source = new MutableRemoteLocationContentSource(
        [
            CreateRecord(
                id: 1,
                name: "Oc Vinh Khanh",
                textVi: "Noi dung Viet",
                textEn: "English text",
                textZh: "Chinese text",
                textDe: "German text")
        ]);
        var stallRepository = new FakeStallContentSyncRepository();
        var translationRepository = new FakeStallTranslationRepository();
        var service = new RemoteLocationSyncService(source, stallRepository, translationRepository);

        var result = await service.SyncAsync();

        Assert.Equal(1, result.SourceRecordCount);
        Assert.Equal(1, result.DistinctSourceRecordCount);
        Assert.Equal(1, result.InsertedStallCount);
        Assert.Equal(0, result.UpdatedStallCount);
        Assert.Equal(0, result.DeactivatedStallCount);
        Assert.Equal(0, result.UnchangedStallCount);
        Assert.Equal(3, result.InsertedTranslationCount);
        Assert.Equal(0, result.UpdatedTranslationCount);
        Assert.Equal(0, result.RemovedTranslationCount);
        Assert.Equal(0, result.UnchangedTranslationCount);
        Assert.Single(stallRepository.StoredStalls);
        Assert.Equal(3, translationRepository.StoredTranslations.Count);
    }

    [Fact]
    public async Task SyncAsync_SecondRunWithSameSnapshot_IsIdempotent()
    {
        var source = new MutableRemoteLocationContentSource(
        [
            CreateRecord(
                id: 1,
                name: "Oc Vinh Khanh",
                textVi: "Noi dung Viet",
                textEn: "English text",
                textZh: "Chinese text",
                textDe: "German text")
        ]);
        var stallRepository = new FakeStallContentSyncRepository();
        var translationRepository = new FakeStallTranslationRepository();
        var service = new RemoteLocationSyncService(source, stallRepository, translationRepository);

        await service.SyncAsync();
        var secondResult = await service.SyncAsync();

        Assert.Equal(0, secondResult.InsertedStallCount);
        Assert.Equal(0, secondResult.UpdatedStallCount);
        Assert.Equal(0, secondResult.DeactivatedStallCount);
        Assert.Equal(1, secondResult.UnchangedStallCount);
        Assert.Equal(0, secondResult.InsertedTranslationCount);
        Assert.Equal(0, secondResult.UpdatedTranslationCount);
        Assert.Equal(0, secondResult.RemovedTranslationCount);
        Assert.Equal(3, secondResult.UnchangedTranslationCount);
        Assert.Single(stallRepository.StoredStalls);
        Assert.Equal(3, translationRepository.StoredTranslations.Count);
    }

    [Fact]
    public async Task SyncAsync_ChangedSourceContent_UpdatesExistingStallAndTranslation()
    {
        var source = new MutableRemoteLocationContentSource(
        [
            CreateRecord(id: 1, name: "Old name", textVi: "Noi dung Viet", textEn: "Old English")
        ]);
        var stallRepository = new FakeStallContentSyncRepository();
        var translationRepository = new FakeStallTranslationRepository();
        var service = new RemoteLocationSyncService(source, stallRepository, translationRepository);

        await service.SyncAsync();

        source.SetRecords(
        [
            CreateRecord(id: 1, name: "New name", textVi: "Noi dung Viet moi", textEn: "New English")
        ]);

        var result = await service.SyncAsync();

        Assert.Equal(0, result.InsertedStallCount);
        Assert.Equal(1, result.UpdatedStallCount);
        Assert.Equal(0, result.DeactivatedStallCount);
        Assert.Equal(0, result.InsertedTranslationCount);
        Assert.Equal(1, result.UpdatedTranslationCount);
        Assert.Equal("New name", stallRepository.StoredStalls[1].Name);
        Assert.Equal("Noi dung Viet moi", stallRepository.StoredStalls[1].NarrationScriptVi);
        Assert.Equal("New English", translationRepository.StoredTranslations[(1, "en")].Description);
    }

    [Fact]
    public async Task SyncAsync_MissingSourceRecord_DeactivatesStall()
    {
        var source = new MutableRemoteLocationContentSource(
        [
            CreateRecord(id: 1, name: "One", textVi: "Vi 1", textEn: "En 1"),
            CreateRecord(id: 2, name: "Two", textVi: "Vi 2", textEn: "En 2")
        ]);
        var stallRepository = new FakeStallContentSyncRepository();
        var translationRepository = new FakeStallTranslationRepository();
        var service = new RemoteLocationSyncService(source, stallRepository, translationRepository);

        await service.SyncAsync();

        source.SetRecords(
        [
            CreateRecord(id: 1, name: "One", textVi: "Vi 1", textEn: "En 1")
        ]);

        var result = await service.SyncAsync();

        Assert.Equal(1, result.DeactivatedStallCount);
        Assert.False(stallRepository.StoredStalls[2].IsActive);
    }

    [Fact]
    public async Task SyncAsync_MissingTranslationInLatestSnapshot_RemovesStoredTranslation()
    {
        var source = new MutableRemoteLocationContentSource(
        [
            CreateRecord(id: 1, name: "One", textVi: "Vi 1", textEn: "En 1", textZh: "Zh 1", textDe: "De 1")
        ]);
        var stallRepository = new FakeStallContentSyncRepository();
        var translationRepository = new FakeStallTranslationRepository();
        var service = new RemoteLocationSyncService(source, stallRepository, translationRepository);

        await service.SyncAsync();

        source.SetRecords(
        [
            CreateRecord(id: 1, name: "One", textVi: "Vi 1", textEn: "En 1", textDe: "De 1")
        ]);

        var result = await service.SyncAsync();

        Assert.Equal(1, result.RemovedTranslationCount);
        Assert.DoesNotContain((1, "zh"), translationRepository.StoredTranslations.Keys);
    }

    private static RemoteLocationRecord CreateRecord(
        int id,
        string name,
        string textVi,
        string? textEn = null,
        string? textZh = null,
        string? textDe = null)
    {
        return new RemoteLocationRecord
        {
            Id = id,
            Name = name,
            Description = $"{name} description",
            Latitude = 10.759,
            Longitude = 106.704,
            TextVi = textVi,
            TextEn = textEn ?? string.Empty,
            TextZh = textZh ?? string.Empty,
            TextDe = textDe ?? string.Empty
        };
    }

    private sealed class MutableRemoteLocationContentSource(IReadOnlyList<RemoteLocationRecord> records) : IRemoteLocationContentSource
    {
        private IReadOnlyList<RemoteLocationRecord> _records = records;

        public void SetRecords(IReadOnlyList<RemoteLocationRecord> records)
        {
            _records = records;
        }

        public Task<IReadOnlyList<RemoteLocationRecord>> GetLocationsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_records);
        }
    }

    private sealed class FakeStallContentSyncRepository : IStallContentSyncRepository
    {
        public Dictionary<int, StallDto> StoredStalls { get; } = [];

        public Task<StallSyncPersistenceResult> UpsertAsync(IReadOnlyCollection<StallDto> stalls, CancellationToken cancellationToken = default)
        {
            var inserted = 0;
            var updated = 0;
            var deactivated = 0;
            var unchanged = 0;
            var sourceIds = stalls.Select(stall => stall.Id).ToHashSet();

            foreach (var existing in StoredStalls.Values.Where(stall => !sourceIds.Contains(stall.Id) && stall.IsActive).ToArray())
            {
                StoredStalls[existing.Id] = Clone(existing, isActive: false);
                deactivated++;
            }

            foreach (var stall in stalls)
            {
                if (!StoredStalls.TryGetValue(stall.Id, out var existing))
                {
                    StoredStalls[stall.Id] = Clone(stall, isActive: true);
                    inserted++;
                    continue;
                }

                var normalized = Clone(stall, isActive: true);

                if (AreEqual(existing, normalized))
                {
                    unchanged++;
                    continue;
                }

                StoredStalls[stall.Id] = normalized;
                updated++;
            }

            return Task.FromResult(new StallSyncPersistenceResult
            {
                InsertedCount = inserted,
                UpdatedCount = updated,
                DeactivatedCount = deactivated,
                UnchangedCount = unchanged
            });
        }

        private static StallDto Clone(StallDto stall, bool? isActive = null)
        {
            return new StallDto
            {
                Id = stall.Id,
                Name = stall.Name,
                DescriptionVi = stall.DescriptionVi,
                Latitude = stall.Latitude,
                Longitude = stall.Longitude,
                TriggerRadiusMeters = stall.TriggerRadiusMeters,
                Priority = stall.Priority,
                Category = stall.Category,
                OpenHours = stall.OpenHours,
                ImageUrl = stall.ImageUrl,
                ImageUrls = stall.ImageUrls.ToArray(),
                Address = stall.Address,
                Phone = stall.Phone,
                ReviewsJson = stall.ReviewsJson,
                MapLink = stall.MapLink,
                NarrationScriptVi = stall.NarrationScriptVi,
                AudioUrl = stall.AudioUrl,
                IsActive = isActive ?? stall.IsActive,
                AverageRating = stall.AverageRating
            };
        }

        private static bool AreEqual(StallDto left, StallDto right)
        {
            return left.Id == right.Id &&
                   left.Name == right.Name &&
                   left.DescriptionVi == right.DescriptionVi &&
                   left.Latitude == right.Latitude &&
                   left.Longitude == right.Longitude &&
                   left.TriggerRadiusMeters == right.TriggerRadiusMeters &&
                   left.Priority == right.Priority &&
                   left.Category == right.Category &&
                   left.OpenHours == right.OpenHours &&
                   left.ImageUrl == right.ImageUrl &&
                   left.ImageUrls.SequenceEqual(right.ImageUrls) &&
                   left.Address == right.Address &&
                   left.Phone == right.Phone &&
                   left.ReviewsJson == right.ReviewsJson &&
                   left.MapLink == right.MapLink &&
                   left.NarrationScriptVi == right.NarrationScriptVi &&
                   left.AudioUrl == right.AudioUrl &&
                   left.IsActive == right.IsActive &&
                   left.AverageRating == right.AverageRating;
        }
    }

    private sealed class FakeStallTranslationRepository : IStallTranslationRepository
    {
        public Dictionary<(int StallId, string LanguageCode), StallTranslationDto> StoredTranslations { get; } = [];

        public Task<IReadOnlyList<StallTranslationDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StallTranslationDto>>(StoredTranslations.Values.ToArray());
        }

        public Task<StallTranslationDto?> GetByStallIdAndLanguageAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            StoredTranslations.TryGetValue((stallId, Normalize(languageCode)), out var translation);
            return Task.FromResult(translation);
        }

        public Task SaveAsync(StallTranslationDto translation, CancellationToken cancellationToken = default)
        {
            StoredTranslations[(translation.StallId, Normalize(translation.LanguageCode))] = new StallTranslationDto
            {
                StallId = translation.StallId,
                LanguageCode = Normalize(translation.LanguageCode),
                Name = translation.Name,
                Description = translation.Description
            };
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
        {
            StoredTranslations.Remove((stallId, Normalize(languageCode)));
            return Task.CompletedTask;
        }

        private static string Normalize(string languageCode)
        {
            return languageCode.Trim().ToLowerInvariant();
        }
    }
}
