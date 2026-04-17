using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class ShellAppNavigator : IAppNavigator
{
    public Task NavigateToStallDetailAsync(ExternalStallRoute route)
    {
        var query = new List<string>
        {
            $"stallId={route.StallId}"
        };

        if (route.AutoPlay)
        {
            query.Add("autoplay=1");
        }

        if (!string.IsNullOrWhiteSpace(route.RequestedLanguageCode))
        {
            query.Add($"lang={Uri.EscapeDataString(route.RequestedLanguageCode)}");
        }

        query.Add($"source={route.SourceType.ToString().ToLowerInvariant()}");

        return Shell.Current.GoToAsync($"stall-detail?{string.Join("&", query)}");
    }
}
