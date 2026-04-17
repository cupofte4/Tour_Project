namespace VinhKhanhGuide.Domain.Entities;

public class StallTranslation
{
    public int Id { get; set; }

    public int StallId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset LastGeneratedAt { get; set; }
}
