using DevToys.Api.Core.Theme;

namespace DevToys.MacOS;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Apply the app color theme.
        MauiProgram.MefComposer.Value.Provider.Import<IThemeListener>().ApplyDesiredColorTheme();

        MainPage = new MainPage();
    }
}
