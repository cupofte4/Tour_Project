using System.Text.Json;
using SQLite;
using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class SqliteStallDataCache : IStallDataCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly SQLiteAsyncConnection _connection;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized;

    public SqliteStallDataCache()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "stall-cache.db3");
        _connection = new SQLiteAsyncConnection(databasePath);
    }

    public async Task<IReadOnlyList<StallSummary>> GetStallsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        cancellationToken.ThrowIfCancellationRequested();

        var records = await _connection.Table<CachedStallRecord>()
            .OrderBy(record => record.ListOrder)
            .ToListAsync();

        return records
            .Where(record => HasStoredJson(record.SummaryJson))
            .Select(record => DeserializeSummary(record.SummaryJson, record.LocalAudioPath))
            .Where(summary => summary is not null)
            .Cast<StallSummary>()
            .ToList();
    }

    public async Task SaveStallsAsync(IReadOnlyList<StallSummary> stalls, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        cancellationToken.ThrowIfCancellationRequested();
        var currentStallIds = stalls.Select(stall => stall.Id).ToHashSet();

        for (var index = 0; index < stalls.Count; index++)
        {
            var stall = stalls[index];
            var existing = await _connection.FindAsync<CachedStallRecord>(stall.Id);
            var persistedSummary = ApplyCachedAudioPath(stall, existing?.LocalAudioPath);

            var record = existing ?? new CachedStallRecord { StallId = stall.Id };
            record.ListOrder = index;
            record.LocalAudioPath = ResolveStoredAudioPath(persistedSummary.LocalAudioPath, existing?.LocalAudioPath);
            record.SummaryJson = JsonSerializer.Serialize(persistedSummary, JsonOptions);

            await _connection.InsertOrReplaceAsync(record);
            await SaveTranslationsAsync(stall.Translations, cancellationToken);
        }

        var cachedRecords = await _connection.Table<CachedStallRecord>().ToListAsync();
        foreach (var staleRecord in cachedRecords.Where(record => !currentStallIds.Contains(record.StallId)))
        {
            await _connection.DeleteAsync<CachedStallRecord>(staleRecord.StallId);
        }
    }

    public async Task<StallDetail?> GetStallDetailAsync(int stallId, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        cancellationToken.ThrowIfCancellationRequested();

        var record = await _connection.FindAsync<CachedStallRecord>(stallId);
        if (record is null)
        {
            return null;
        }

        if (HasStoredJson(record.DetailJson))
        {
            return DeserializeDetail(record.DetailJson, record.LocalAudioPath);
        }

        if (HasStoredJson(record.SummaryJson))
        {
            var summary = DeserializeSummary(record.SummaryJson, record.LocalAudioPath);
            return summary is null ? null : ToDetail(summary);
        }

        return null;
    }

    public async Task SaveStallDetailAsync(StallDetail stallDetail, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        cancellationToken.ThrowIfCancellationRequested();

        var existing = await _connection.FindAsync<CachedStallRecord>(stallDetail.Id);
        var persistedDetail = ApplyCachedAudioPath(stallDetail, existing?.LocalAudioPath);

        var record = existing ?? new CachedStallRecord { StallId = stallDetail.Id };
        record.LocalAudioPath = ResolveStoredAudioPath(persistedDetail.LocalAudioPath, existing?.LocalAudioPath);
        record.DetailJson = JsonSerializer.Serialize(persistedDetail, JsonOptions);
        record.SummaryJson = JsonSerializer.Serialize(ToSummary(persistedDetail), JsonOptions);

        await _connection.InsertOrReplaceAsync(record);
        await SaveTranslationsAsync(stallDetail.Translations, cancellationToken);
    }

    public async Task UpdateLocalAudioPathAsync(int stallId, string? localAudioPath, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        cancellationToken.ThrowIfCancellationRequested();

        var record = await _connection.FindAsync<CachedStallRecord>(stallId);
        if (record is null)
        {
            return;
        }

        var normalizedAudioPath = NormalizeStoredAudioPath(localAudioPath);
        record.LocalAudioPath = normalizedAudioPath;

        if (HasStoredJson(record.SummaryJson))
        {
            var summary = JsonSerializer.Deserialize<StallSummary>(record.SummaryJson, JsonOptions);
            if (summary is not null)
            {
                record.SummaryJson = JsonSerializer.Serialize(CloneSummaryWithAudioPath(summary, normalizedAudioPath), JsonOptions);
            }
        }

        if (HasStoredJson(record.DetailJson))
        {
            var detail = JsonSerializer.Deserialize<StallDetail>(record.DetailJson, JsonOptions);
            if (detail is not null)
            {
                record.DetailJson = JsonSerializer.Serialize(CloneDetailWithAudioPath(detail, normalizedAudioPath), JsonOptions);
            }
        }

        await _connection.InsertOrReplaceAsync(record);
    }

    public async Task<StallTranslation?> GetTranslationAsync(int stallId, string languageCode, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedLanguageCode = NormalizeLanguageCode(languageCode);
        if (string.IsNullOrWhiteSpace(normalizedLanguageCode))
        {
            return null;
        }

        var recordKey = CreateTranslationKey(stallId, normalizedLanguageCode);
        var record = await _connection.FindAsync<CachedTranslationRecord>(recordKey);
        return record is null || !HasStoredJson(record.TranslationJson)
            ? null
            : JsonSerializer.Deserialize<StallTranslation>(record.TranslationJson, JsonOptions);
    }

    public async Task SaveTranslationAsync(StallTranslation translation, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedLanguageCode = NormalizeLanguageCode(translation.LanguageCode);
        if (translation.StallId <= 0 || string.IsNullOrWhiteSpace(normalizedLanguageCode))
        {
            return;
        }

        var normalizedTranslation = new StallTranslation
        {
            StallId = translation.StallId,
            LanguageCode = normalizedLanguageCode,
            Name = translation.Name,
            Description = translation.Description
        };

        await _connection.InsertOrReplaceAsync(new CachedTranslationRecord
        {
            Key = CreateTranslationKey(translation.StallId, normalizedLanguageCode),
            StallId = translation.StallId,
            LanguageCode = normalizedLanguageCode,
            TranslationJson = JsonSerializer.Serialize(normalizedTranslation, JsonOptions)
        });
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializationLock.WaitAsync();

        try
        {
            if (_isInitialized)
            {
                return;
            }

            await _connection.CreateTableAsync<CachedStallRecord>();
            await _connection.CreateTableAsync<CachedTranslationRecord>();
            _isInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task SaveTranslationsAsync(IReadOnlyList<StallTranslation> translations, CancellationToken cancellationToken)
    {
        foreach (var translation in translations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SaveTranslationAsync(translation, cancellationToken);
        }
    }

    private static StallSummary? DeserializeSummary(string json, string? cachedAudioPath)
    {
        var summary = JsonSerializer.Deserialize<StallSummary>(json, JsonOptions);
        return summary is null ? null : ApplyCachedAudioPath(summary, cachedAudioPath);
    }

    private static StallDetail? DeserializeDetail(string json, string? cachedAudioPath)
    {
        var detail = JsonSerializer.Deserialize<StallDetail>(json, JsonOptions);
        return detail is null ? null : ApplyCachedAudioPath(detail, cachedAudioPath);
    }

    private static StallSummary ApplyCachedAudioPath(StallSummary stall, string? cachedAudioPath)
    {
        var audioPath = ResolveStoredAudioPath(stall.LocalAudioPath, cachedAudioPath);
        return CloneSummaryWithAudioPath(stall, audioPath);
    }

    private static StallDetail ApplyCachedAudioPath(StallDetail stall, string? cachedAudioPath)
    {
        var audioPath = ResolveStoredAudioPath(stall.LocalAudioPath, cachedAudioPath);
        return CloneDetailWithAudioPath(stall, audioPath);
    }

    private static StallDetail ToDetail(StallSummary summary)
    {
        return new StallDetail
        {
            Id = summary.Id,
            Name = summary.Name,
            DescriptionVi = summary.DescriptionVi,
            Latitude = summary.Latitude,
            Longitude = summary.Longitude,
            TriggerRadiusMeters = summary.TriggerRadiusMeters,
            Priority = summary.Priority,
            OpenHours = summary.OpenHours,
            Category = summary.Category,
            ImageUrl = summary.ImageUrl,
            MapLink = summary.MapLink,
            NarrationScriptVi = summary.NarrationScriptVi,
            AudioUrl = summary.AudioUrl,
            LocalAudioPath = summary.LocalAudioPath,
            IsActive = summary.IsActive,
            AverageRating = summary.AverageRating,
            Translations = summary.Translations
        };
    }

    private static StallSummary ToSummary(StallDetail detail)
    {
        return new StallSummary
        {
            Id = detail.Id,
            Name = detail.Name,
            DescriptionVi = detail.DescriptionVi,
            Latitude = detail.Latitude,
            Longitude = detail.Longitude,
            TriggerRadiusMeters = detail.TriggerRadiusMeters,
            Priority = detail.Priority,
            Category = detail.Category,
            OpenHours = detail.OpenHours,
            ImageUrl = detail.ImageUrl,
            MapLink = detail.MapLink,
            NarrationScriptVi = detail.NarrationScriptVi,
            AudioUrl = detail.AudioUrl,
            LocalAudioPath = detail.LocalAudioPath,
            IsActive = detail.IsActive,
            AverageRating = detail.AverageRating,
            Translations = detail.Translations
        };
    }

    private static string ResolveStoredAudioPath(string? currentPath, string? cachedPath)
    {
        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            return NormalizeStoredAudioPath(currentPath);
        }

        return NormalizeStoredAudioPath(cachedPath);
    }

    private static string NormalizeStoredAudioPath(string? audioPath)
    {
        return string.IsNullOrWhiteSpace(audioPath)
            ? string.Empty
            : audioPath.Trim();
    }

    private static StallSummary CloneSummaryWithAudioPath(StallSummary stall, string localAudioPath)
    {
        return new StallSummary
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
            MapLink = stall.MapLink,
            NarrationScriptVi = stall.NarrationScriptVi,
            AudioUrl = stall.AudioUrl,
            LocalAudioPath = localAudioPath,
            IsActive = stall.IsActive,
            AverageRating = stall.AverageRating,
            Translations = stall.Translations
        };
    }

    private static StallDetail CloneDetailWithAudioPath(StallDetail stall, string localAudioPath)
    {
        return new StallDetail
        {
            Id = stall.Id,
            Name = stall.Name,
            DescriptionVi = stall.DescriptionVi,
            Latitude = stall.Latitude,
            Longitude = stall.Longitude,
            TriggerRadiusMeters = stall.TriggerRadiusMeters,
            Priority = stall.Priority,
            OpenHours = stall.OpenHours,
            Category = stall.Category,
            ImageUrl = stall.ImageUrl,
            MapLink = stall.MapLink,
            NarrationScriptVi = stall.NarrationScriptVi,
            AudioUrl = stall.AudioUrl,
            LocalAudioPath = localAudioPath,
            IsActive = stall.IsActive,
            AverageRating = stall.AverageRating,
            Translations = stall.Translations
        };
    }

    private static string NormalizeLanguageCode(string languageCode)
    {
        return string.IsNullOrWhiteSpace(languageCode)
            ? string.Empty
            : languageCode.Trim().ToLowerInvariant();
    }

    private static string CreateTranslationKey(int stallId, string languageCode)
    {
        return $"{stallId}:{languageCode}";
    }

    internal static bool HasStoredJson(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private sealed class CachedStallRecord
    {
        [PrimaryKey]
        public int StallId { get; set; }

        public int ListOrder { get; set; }

        public string SummaryJson { get; set; } = string.Empty;

        public string DetailJson { get; set; } = string.Empty;

        public string LocalAudioPath { get; set; } = string.Empty;
    }

    private sealed class CachedTranslationRecord
    {
        [PrimaryKey]
        public string Key { get; set; } = string.Empty;

        public int StallId { get; set; }

        public string LanguageCode { get; set; } = string.Empty;

        public string TranslationJson { get; set; } = string.Empty;
    }
}
