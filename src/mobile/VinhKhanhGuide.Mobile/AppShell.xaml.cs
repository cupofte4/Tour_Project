namespace VinhKhanhGuide.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("stall-detail", typeof(Views.StallDetailPage));
        Routing.RegisterRoute("stall-map", typeof(Views.StallMapPage));
    }
}
