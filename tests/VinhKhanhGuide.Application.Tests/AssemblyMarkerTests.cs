namespace VinhKhanhGuide.Application.Tests;

public class AssemblyMarkerTests
{
    [Fact]
    public void ApplicationAssemblyMarker_CanBeConstructed()
    {
        var marker = new VinhKhanhGuide.Application.AssemblyMarker();

        Assert.NotNull(marker);
    }
}
