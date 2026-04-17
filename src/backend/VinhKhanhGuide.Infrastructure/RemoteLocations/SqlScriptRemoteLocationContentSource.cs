using System.Globalization;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Application.RemoteLocations;

namespace VinhKhanhGuide.Infrastructure.RemoteLocations;

public sealed class SqlScriptRemoteLocationContentSource(
    IOptions<RemoteLocationImportOptions> options) : IRemoteLocationContentSource
{
    public Task<IReadOnlyList<RemoteLocationRecord>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var snapshotPath = ResolveSnapshotPath(options.Value.SqlSnapshotPath);

        if (!File.Exists(snapshotPath))
        {
            return Task.FromResult<IReadOnlyList<RemoteLocationRecord>>([]);
        }

        var sql = File.ReadAllText(snapshotPath);
        var locations = ParseLocations(sql);
        return Task.FromResult<IReadOnlyList<RemoteLocationRecord>>(locations);
    }

    private static string ResolveSnapshotPath(string configuredPath)
    {
        var candidateFileName = string.IsNullOrWhiteSpace(configuredPath)
            ? "foodguide.sql"
            : configuredPath;

        if (Path.IsPathRooted(candidateFileName))
        {
            return candidateFileName;
        }

        foreach (var root in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var current = new DirectoryInfo(root);

            while (current is not null)
            {
                var candidatePath = Path.Combine(current.FullName, candidateFileName);

                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }

                current = current.Parent;
            }
        }

        return Path.GetFullPath(candidateFileName, Directory.GetCurrentDirectory());
    }

    private static IReadOnlyList<RemoteLocationRecord> ParseLocations(string sql)
    {
        var insertIndex = sql.IndexOf("INSERT INTO Locations", StringComparison.OrdinalIgnoreCase);

        if (insertIndex < 0)
        {
            return [];
        }

        var valuesIndex = sql.IndexOf("VALUES", insertIndex, StringComparison.OrdinalIgnoreCase);

        if (valuesIndex < 0)
        {
            return [];
        }

        var columnsStart = sql.IndexOf('(', insertIndex);
        var columnsEnd = sql.IndexOf(')', columnsStart + 1);

        if (columnsStart < 0 || columnsEnd < 0 || columnsEnd <= columnsStart)
        {
            return [];
        }

        var columns = sql[(columnsStart + 1)..columnsEnd]
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var statementEnd = FindStatementTerminator(sql, valuesIndex);
        var valuesSection = sql[(valuesIndex + "VALUES".Length)..statementEnd];
        var rows = ParseRows(valuesSection);

        return rows
            .Select((row, index) => MapRow(columns, row, index + 1))
            .Where(record => record is not null)
            .Cast<RemoteLocationRecord>()
            .ToArray();
    }

    private static int FindStatementTerminator(string sql, int startIndex)
    {
        var inString = false;

        for (var index = startIndex; index < sql.Length; index++)
        {
            var current = sql[index];

            if (current == '\'')
            {
                if (inString && index + 1 < sql.Length && sql[index + 1] == '\'')
                {
                    index++;
                    continue;
                }

                inString = !inString;
                continue;
            }

            if (!inString && current == ';')
            {
                return index;
            }
        }

        return sql.Length;
    }

    private static IReadOnlyList<IReadOnlyList<string?>> ParseRows(string valuesSection)
    {
        var rows = new List<IReadOnlyList<string?>>();
        List<string?>? currentRow = null;
        var token = new System.Text.StringBuilder();
        var inString = false;

        for (var index = 0; index < valuesSection.Length; index++)
        {
            var current = valuesSection[index];

            if (current == '\'')
            {
                token.Append(current);

                if (inString && index + 1 < valuesSection.Length && valuesSection[index + 1] == '\'')
                {
                    token.Append(valuesSection[index + 1]);
                    index++;
                    continue;
                }

                inString = !inString;
                continue;
            }

            if (inString)
            {
                token.Append(current);
                continue;
            }

            if (current == '(')
            {
                currentRow = [];
                token.Clear();
                continue;
            }

            if (current == ',' && currentRow is not null)
            {
                currentRow.Add(ParseToken(token.ToString()));
                token.Clear();
                continue;
            }

            if (current == ')' && currentRow is not null)
            {
                currentRow.Add(ParseToken(token.ToString()));
                rows.Add(currentRow);
                currentRow = null;
                token.Clear();
                continue;
            }

            if (!char.IsWhiteSpace(current) || token.Length > 0)
            {
                token.Append(current);
            }
        }

        return rows;
    }

    private static string? ParseToken(string token)
    {
        var trimmed = token.Trim();

        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (string.Equals(trimmed, "NULL", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (trimmed.Length >= 2 && trimmed[0] == '\'' && trimmed[^1] == '\'')
        {
            return trimmed[1..^1].Replace("''", "'", StringComparison.Ordinal);
        }

        return trimmed;
    }

    private static RemoteLocationRecord? MapRow(IReadOnlyList<string> columns, IReadOnlyList<string?> values, int fallbackId)
    {
        if (values.Count != columns.Count)
        {
            return null;
        }

        var data = columns
            .Select((column, index) => new KeyValuePair<string, string?>(column, values[index]))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);

        return new RemoteLocationRecord
        {
            Id = data.ContainsKey("Id")
                ? ParseInt(data, "Id")
                : fallbackId,
            Name = GetString(data, "Name"),
            Description = GetString(data, "Description"),
            Image = GetString(data, "Image"),
            Images = GetString(data, "Images"),
            Address = GetString(data, "Address"),
            Phone = GetString(data, "Phone"),
            ReviewsJson = GetString(data, "ReviewsJson"),
            Latitude = ParseDouble(data, "Latitude"),
            Longitude = ParseDouble(data, "Longitude"),
            TextVi = GetString(data, "TextVi"),
            TextEn = GetString(data, "TextEn"),
            TextZh = GetString(data, "TextZh"),
            TextDe = GetString(data, "TextDe")
        };
    }

    private static string GetString(IReadOnlyDictionary<string, string?> data, string key)
    {
        return data.TryGetValue(key, out var value) && value is not null ? value : string.Empty;
    }

    private static int ParseInt(IReadOnlyDictionary<string, string?> data, string key)
    {
        return data.TryGetValue(key, out var value) &&
               int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
            ? parsedValue
            : 0;
    }

    private static double ParseDouble(IReadOnlyDictionary<string, string?> data, string key)
    {
        return data.TryGetValue(key, out var value) &&
               double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue)
            ? parsedValue
            : 0d;
    }
}
