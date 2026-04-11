using VinhKhanhGuide.Application.RemoteLocations;

namespace VinhKhanhGuide.Application.Tests;

public class RemoteLocationToStallMapperTests
{
    [Fact]
    public void Map_UsesRemoteFieldsAndSafeDefaults()
    {
        var location = new RemoteLocationRecord
        {
            Id = 7,
            Name = "Oc Vinh Khanh",
            Description = "Fallback description",
            Image = string.Empty,
            Images = "[\"https://cdn.example.com/1.jpg\",\"https://cdn.example.com/2.jpg\"]",
            Address = "16 Vinh Khanh, District 4",
            ReviewsJson = "[{\"rating\":5},{\"rating\":4}]",
            Latitude = 10.7593,
            Longitude = 106.7046,
            TextVi = "Noi dung tieng Viet",
            TextEn = "English narration",
            TextZh = "Chinese narration"
        };

        var result = RemoteLocationToStallMapper.Map(location);

        Assert.Equal(7, result.Stall.Id);
        Assert.Equal("Oc Vinh Khanh", result.Stall.Name);
        Assert.Equal("Fallback description", result.Stall.DescriptionVi);
        Assert.Equal("https://cdn.example.com/1.jpg", result.Stall.ImageUrl);
        Assert.Equal(2, result.Stall.ImageUrls.Count);
        Assert.Equal("16 Vinh Khanh, District 4", result.Stall.Address);
        Assert.Equal(40d, result.Stall.TriggerRadiusMeters);
        Assert.Equal(100, result.Stall.Priority);
        Assert.Equal(4.5m, result.Stall.AverageRating);
        Assert.Equal(2, result.Translations.Count);
        Assert.Contains(result.Translations, translation => translation.LanguageCode == "en");
        Assert.Contains(result.Translations, translation => translation.LanguageCode == "zh");
    }

    [Fact]
    public void ParseJsonStringArray_ReturnsEmpty_WhenJsonIsInvalid()
    {
        var result = RemoteLocationToStallMapper.ParseJsonStringArray("not-json");

        Assert.Empty(result);
    }
}
