using DevToys.Api;

namespace DevToys.MacOS.Core;

[Export(typeof(IThemeListener))]
internal sealed class ThemeListener : IThemeListener
{
    private readonly ISettingsProvider _settingsProvider;

    [ImportingConstructor]
    public ThemeListener(ISettingsProvider settingsProvider)
    {
        // Listen for app settings
        _settingsProvider = settingsProvider;
        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

        // Listen for operating system settings.
        Guard.IsNotNull(Application.Current);
        Application.Current.RequestedThemeChanged += System_RequestedThemeChanged;

        UpdateSystemSettingsAndApplyTheme();
    }

    public AvailableApplicationTheme CurrentSystemTheme { get; private set; }

    public AvailableApplicationTheme CurrentAppTheme => _settingsProvider.GetSetting(DevToys.Core.Settings.PredefinedSettings.Theme);

    public ApplicationTheme ActualAppTheme { get; private set; }

    public bool IsHighContrast { get; private set; }

    public bool IsCompactMode { get; private set; }

    public bool UserIsCompactModePreference => _settingsProvider.GetSetting(PredefinedSettings.CompactMode);

    public event EventHandler? ThemeChanged;

    public void ApplyDesiredColorTheme()
    {
        Guard.IsNotNull(Application.Current);
        AvailableApplicationTheme theme = CurrentAppTheme;

        if (theme == AvailableApplicationTheme.Default)
        {
            theme = CurrentSystemTheme;

            // This makes System_RequestedThemeChanged reacting to the system theme change
            // and makes the app titlebar following the system theme.
            Application.Current.UserAppTheme = AppTheme.Unspecified;

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
        else
        {
            // Set theme for window root.
            if (theme == AvailableApplicationTheme.Dark)
            {
                ActualAppTheme = ApplicationTheme.Dark;
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
            else
            {
                ActualAppTheme = ApplicationTheme.Light;
                Application.Current.UserAppTheme = AppTheme.Light;
            }
        }

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Evaluates whether the theme should automatically be updated or not, based on app and operating system settings.
    /// </summary>
    private void UpdateSystemSettingsAndApplyTheme()
    {
        IsHighContrast = false; // TODO: Detect high contrast
        CurrentSystemTheme = GetCurrentSystemTheme();

        ApplyDesiredColorTheme();
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

    private void System_RequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        UpdateSystemSettingsAndApplyTheme();
    }

    private static AvailableApplicationTheme GetCurrentSystemTheme()
    {
        Guard.IsNotNull(Application.Current);
        AppTheme currentSystemTheme = Application.Current.RequestedTheme;

        return currentSystemTheme == AppTheme.Light ? AvailableApplicationTheme.Light : AvailableApplicationTheme.Dark;
    }
}
