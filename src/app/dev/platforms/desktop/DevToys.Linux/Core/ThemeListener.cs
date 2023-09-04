using DevToys.Api;

namespace DevToys.Linux.Core;

[Export(typeof(IThemeListener))]
internal sealed class ThemeListener : IThemeListener
{
    public AvailableApplicationTheme CurrentSystemTheme => throw new NotImplementedException();

    public AvailableApplicationTheme CurrentAppTheme => throw new NotImplementedException();

    public ApplicationTheme ActualAppTheme => throw new NotImplementedException();

    public bool IsHighContrast => throw new NotImplementedException();

    public bool IsCompactMode => throw new NotImplementedException();

    public bool UserIsCompactModePreference => throw new NotImplementedException();

    public event EventHandler? ThemeChanged;

    public void ApplyDesiredColorTheme()
    {
        throw new NotImplementedException();
    }
}
