using DevToys.Api;
using DevToys.Api.Core.Theme;
using DevToys.Core.Settings;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;

namespace DevToys.Wasdk.Core.Theme;

[Export(typeof(IThemeListener))]
internal sealed class ThemeListener : IThemeListener
{
    private readonly AccessibilitySettings _accessible = new();
    private readonly ISettingsProvider _settingsProvider;

    [ImportingConstructor]
    public ThemeListener(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;

        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

        if (Application.Current.RequestedTheme == Microsoft.UI.Xaml.ApplicationTheme.Dark)
        {
            CurrentSystemTheme = AvailableApplicationTheme.Dark;
        }
        else
        {
            CurrentSystemTheme = AvailableApplicationTheme.Light;
        }

        IsHighContrast = _accessible.HighContrast;
    }

    public AvailableApplicationTheme CurrentSystemTheme { get; private set; }

    public AvailableApplicationTheme CurrentAppTheme => _settingsProvider.GetSetting(PredefinedSettings.Theme);

    public Api.Core.Theme.ApplicationTheme ActualAppTheme { get; private set; }

    public bool IsHighContrast { get; private set; }

    public event EventHandler? ThemeChanged;

    public void UpdateThemeIfNeeded()
    {
        AvailableApplicationTheme currentAppTheme;
        if (Application.Current.RequestedTheme == Microsoft.UI.Xaml.ApplicationTheme.Dark)
        {
            currentAppTheme = AvailableApplicationTheme.Dark;
        }
        else
        {
            currentAppTheme = AvailableApplicationTheme.Light;
        }

        if (CurrentSystemTheme != currentAppTheme || IsHighContrast != _accessible.HighContrast)
        {
            UpdateProperties();
        }
    }

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
            ActualAppTheme = Api.Core.Theme.ApplicationTheme.Dark;
        }
        else
        {
            ActualAppTheme = Api.Core.Theme.ApplicationTheme.Light;
        }
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (string.Equals(PredefinedSettings.Theme.Name, e.SettingName, StringComparison.Ordinal))
        {
            ApplyDesiredColorTheme();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Set our current properties and fire a change notification.
    /// </summary>
    private void UpdateProperties()
    {
        // TODO: Not sure if HighContrastScheme names are localized?
        if (_accessible.HighContrast && _accessible.HighContrastScheme.IndexOf("white", StringComparison.OrdinalIgnoreCase) != -1)
        {
            IsHighContrast = false;
            CurrentSystemTheme = AvailableApplicationTheme.Light;
        }
        else
        {
            // Otherwise, we just set to what's in the system as we'd expect.
            IsHighContrast = _accessible.HighContrast;

            AvailableApplicationTheme currentAppTheme;
            if (Application.Current.RequestedTheme == Microsoft.UI.Xaml.ApplicationTheme.Dark)
            {
                currentAppTheme = AvailableApplicationTheme.Dark;
            }
            else
            {
                currentAppTheme = AvailableApplicationTheme.Light;
            }

            CurrentSystemTheme = currentAppTheme;
        }

        ApplyDesiredColorTheme();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
