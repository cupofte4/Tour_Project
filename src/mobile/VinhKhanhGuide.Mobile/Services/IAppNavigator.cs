using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IAppNavigator
{
    Task NavigateToStallDetailAsync(ExternalStallRoute route);
}
