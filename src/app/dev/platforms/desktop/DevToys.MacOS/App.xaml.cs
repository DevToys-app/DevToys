using DevToys.Api;
using DevToys.Blazor.Core.Services;
using DevToys.Core;
using DevToys.MacOS.Core;

namespace DevToys.MacOS;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        Guard.IsNotNull(MauiProgram.MefComposer);
        var titleBarInfoProvider = MauiProgram.MefComposer.Provider.Import<TitleBarInfoProvider>();

        Guard.IsNotNull(MauiProgram.ServiceProvider);
        var windowService = (WindowService)MauiProgram.ServiceProvider.GetService<IWindowService>()!;
        windowService.SetWindow(window, titleBarInfoProvider);

        return window;
    }
}
