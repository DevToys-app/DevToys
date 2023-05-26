using Foundation;

namespace DevToys.MacOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private readonly MauiProgram _program = new();

    protected override MauiApp CreateMauiApp() => _program.CreateMauiApp();
}
