namespace VinhKhanhGuide.Mobile.Models;

public static class DemoUiOptions
{
    public static bool ShowDebugTools
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
