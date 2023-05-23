using DevToys.Api;
using DevToys.Api.Core.Theme;

namespace DevToys.MacOS.Core;

[Export(typeof(IThemeListener))]
internal sealed class ThemeListener : IThemeListener
{
    private readonly ISettingsProvider _settingsProvider;

    [ImportingConstructor]
    public ThemeListener(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;

        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

        Guard.IsNotNull(Application.Current);
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            CurrentSystemTheme = AvailableApplicationTheme.Dark;
        }
        else
        {
            CurrentSystemTheme = AvailableApplicationTheme.Light;
        }

        // TODO: Support High contrast
        IsHighContrast = false;
    }

    public AvailableApplicationTheme CurrentSystemTheme { get; private set; }

    public AvailableApplicationTheme CurrentAppTheme => _settingsProvider.GetSetting(DevToys.Core.Settings.PredefinedSettings.Theme);

    public ApplicationTheme ActualAppTheme { get; private set; }

    public bool IsHighContrast { get; private set; }

    public bool IsCompactMode => _settingsProvider.GetSetting(DevToys.Api.PredefinedSettings.CompactMode);

    public event EventHandler? ThemeChanged;

    public void ApplyDesiredColorTheme()
    {
        AvailableApplicationTheme theme = CurrentAppTheme;

        if (theme == AvailableApplicationTheme.Default)
        {
            theme = CurrentSystemTheme;
        }

        // Set theme for window root.
        if (theme == AvailableApplicationTheme.Dark)
        {
            ActualAppTheme = ApplicationTheme.Dark;
        }
        else
        {
            ActualAppTheme = ApplicationTheme.Light;
        }
    }

    public void UpdateThemeIfNeeded()
    {
        AvailableApplicationTheme currentAppTheme;
        Guard.IsNotNull(Application.Current);
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            currentAppTheme = AvailableApplicationTheme.Dark;
        }
        else
        {
            currentAppTheme = AvailableApplicationTheme.Light;
        }

        if (CurrentSystemTheme != currentAppTheme /*|| IsHighContrast != _accessible.HighContrast // TODO */)
        {
            UpdateProperties();
        }
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (string.Equals(DevToys.Core.Settings.PredefinedSettings.Theme.Name, e.SettingName, StringComparison.Ordinal))
        {
            ApplyDesiredColorTheme();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (string.Equals(DevToys.Api.PredefinedSettings.CompactMode.Name, e.SettingName, StringComparison.Ordinal))
        {
            // TODO: Apply the mode.
        }
    }

    /// <summary>
    /// Set our current properties and fire a change notification.
    /// </summary>
    private void UpdateProperties()
    {
        // TODO: Support High contrast
        IsHighContrast = false;

        AvailableApplicationTheme currentAppTheme;
        Guard.IsNotNull(Application.Current);
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            currentAppTheme = AvailableApplicationTheme.Dark;
        }
        else
        {
            currentAppTheme = AvailableApplicationTheme.Light;
        }

        CurrentSystemTheme = currentAppTheme;

        ApplyDesiredColorTheme();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
