namespace VinhKhanhGuide.Mobile.Models;

public static class StallImageSourceResolver
{
    public const string PlaceholderImageSource = "stall-placeholder.svg";

    public static string Resolve(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return PlaceholderImageSource;
        }

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            return imageUrl;
        }

        return string.Equals(uri.Host, "example.com", StringComparison.OrdinalIgnoreCase)
            ? PlaceholderImageSource
            : imageUrl;
    }
}
