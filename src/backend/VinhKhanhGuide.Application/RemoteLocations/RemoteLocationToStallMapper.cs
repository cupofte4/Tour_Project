using System.Globalization;
using System.Text.Json;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Application.RemoteLocations;

public static class RemoteLocationToStallMapper
{
    private const double DefaultTriggerRadiusMeters = 40d;
    private const int DefaultPriority = 100;
    private const string DefaultCategory = "Food Stall";
    private const string DefaultOpenHours = "Open hours unavailable";

    public static MappedRemoteLocationContent Map(RemoteLocationRecord location)
    {
        var imageUrls = ParseJsonStringArray(location.Images);
        var primaryImage = !string.IsNullOrWhiteSpace(location.Image)
            ? location.Image
            : imageUrls.FirstOrDefault() ?? string.Empty;
        var shortDescription = FirstNonEmpty(location.Description, location.TextVi);
        var vietnameseText = FirstNonEmpty(location.TextVi, location.Description);
        var stall = new StallDto
        {
            Id = location.Id,
            Name = location.Name,
            DescriptionVi = shortDescription,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            TriggerRadiusMeters = DefaultTriggerRadiusMeters,
            Priority = DefaultPriority,
            Category = DefaultCategory,
            OpenHours = DefaultOpenHours,
            ImageUrl = primaryImage,
            ImageUrls = imageUrls.Count > 0 ? imageUrls : primaryImage is not "" ? [primaryImage] : [],
            Address = location.Address,
            Phone = location.Phone,
            ReviewsJson = location.ReviewsJson,
            MapLink = BuildMapLink(location),
            NarrationScriptVi = vietnameseText,
            AudioUrl = string.Empty,
            IsActive = true,
            AverageRating = ParseAverageRating(location.ReviewsJson)
        };

        var translations = BuildTranslations(location);
        return new MappedRemoteLocationContent(stall, translations);
    }

    private static IReadOnlyList<StallTranslationDto> BuildTranslations(RemoteLocationRecord location)
    {
        var translations = new List<StallTranslationDto>();

        AddTranslationIfPresent(translations, location.Id, "en", location.Name, location.TextEn);
        AddTranslationIfPresent(translations, location.Id, "zh", location.Name, location.TextZh);
        AddTranslationIfPresent(translations, location.Id, "de", location.Name, location.TextDe);

        return translations;
    }

    private static void AddTranslationIfPresent(
        ICollection<StallTranslationDto> translations,
        int stallId,
        string languageCode,
        string name,
        string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        translations.Add(new StallTranslationDto
        {
            StallId = stallId,
            LanguageCode = languageCode,
            Name = name,
            Description = description
        });
    }

    private static string BuildMapLink(RemoteLocationRecord location)
    {
        var label = !string.IsNullOrWhiteSpace(location.Address)
            ? $"{location.Name}, {location.Address}"
            : location.Name;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"https://maps.apple.com/?ll={location.Latitude},{location.Longitude}&q={Uri.EscapeDataString(label)}");
    }

    private static decimal ParseAverageRating(string reviewsJson)
    {
        if (string.IsNullOrWhiteSpace(reviewsJson))
        {
            return 0m;
        }

        try
        {
            using var document = JsonDocument.Parse(reviewsJson);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return 0m;
            }

            var ratings = document.RootElement
                .EnumerateArray()
                .Select(review =>
                {
                    if (!review.TryGetProperty("rating", out var ratingProperty))
                    {
                        return (decimal?)null;
                    }

                    return ratingProperty.ValueKind switch
                    {
                        JsonValueKind.Number when ratingProperty.TryGetDecimal(out var decimalRating) => decimalRating,
                        JsonValueKind.String when decimal.TryParse(
                            ratingProperty.GetString(),
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out var parsedStringRating) => parsedStringRating,
                        _ => null
                    };
                })
                .Where(rating => rating.HasValue)
                .Select(rating => rating!.Value)
                .ToArray();

            if (ratings.Length == 0)
            {
                return 0m;
            }

            var average = ratings.Average();
            return Math.Round(average, 2, MidpointRounding.AwayFromZero);
        }
        catch
        {
            return 0m;
        }
    }

    public static IReadOnlyList<string> ParseJsonStringArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<string>>(json);
            return items?
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray() ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }
}

public sealed record MappedRemoteLocationContent(
    StallDto Stall,
    IReadOnlyList<StallTranslationDto> Translations);
