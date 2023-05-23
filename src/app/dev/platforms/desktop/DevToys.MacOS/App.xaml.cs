using DevToys.Api;

namespace DevToys.MacOS;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Apply the app color theme.
        Guard.IsNotNull(MauiProgram.MefComposer);
        MauiProgram.MefComposer.Provider.Import<IThemeListener>().ApplyDesiredColorTheme();

        MainPage = new MainPage();
    }
}
