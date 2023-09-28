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

        // TODO: When the app exit, call GuiToolProvider.DisposeTools().
        // TODO: Invoke FileHelper.ClearTempFiles(Constants.AppTempFolder); when the app exit.
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
