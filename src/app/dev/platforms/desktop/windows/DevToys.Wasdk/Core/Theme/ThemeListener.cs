using DevToys.Api;
using DevToys.Api.Core.Theme;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;

namespace DevToys.Wasdk.Core.Theme;

[Export(typeof(IThemeListener))]
internal sealed class ThemeListener : IThemeListener
{
    private readonly AccessibilitySettings _accessible = new();
    private readonly ISettingsProvider _settingsProvider;
    private readonly ResourceDictionary _compactModeResourceDictionary = new();

    [ImportingConstructor]
    public ThemeListener(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;

        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

        _compactModeResourceDictionary.Source = new Uri("ms-appx:///DevToys.UI.Framework/Themes/Generic.xaml");

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

    public AvailableApplicationTheme CurrentAppTheme => _settingsProvider.GetSetting(DevToys.Core.Settings.PredefinedSettings.Theme);

    public Api.Core.Theme.ApplicationTheme ActualAppTheme { get; private set; }

    public bool IsHighContrast { get; private set; }

    public bool IsCompactMode => _settingsProvider.GetSetting(DevToys.Api.PredefinedSettings.CompactMode);

    public event EventHandler? ThemeChanged;

    public void UpdateThemeIfNeeded()
    {
        UpdateCompactMode();

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
        if (string.Equals(DevToys.Core.Settings.PredefinedSettings.Theme.Name, e.SettingName, StringComparison.Ordinal))
        {
            ApplyDesiredColorTheme();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (string.Equals(DevToys.Api.PredefinedSettings.CompactMode.Name, e.SettingName, StringComparison.Ordinal))
        {
            UpdateCompactMode();
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

    private void UpdateCompactMode()
    {
        if (IsCompactMode)
        {
            // NavigationView
            UpdateAppResource("NavigationViewItemOnLeftMinHeight", 20);

            // Page
            UpdateAppResource("PageDefaultPadding", new Thickness(16, 32, 16, 32));

            // UISetting
            UpdateAppResource("UISettingPresenterContentPadding", new Thickness(16, 4, 16, 4));
            UpdateAppResource("UISettingGroupPresenterContentMargin", new Thickness(0, 4, 0, 4));

            // ComboBox
            UpdateAppResource("ComboBoxMinHeight", 24);
            UpdateAppResource("ComboBoxPadding", new Thickness(12, 1, 0, 3));
        }
        else
        {
            // NavigationView
            UpdateAppResource("NavigationViewItemOnLeftMinHeight", 32);

            // Page
            UpdateAppResource("PageDefaultPadding", new Thickness(56, 32, 56, 56));

            // UISetting
            UpdateAppResource("UISettingPresenterContentPadding", new Thickness(16));
            UpdateAppResource("UISettingGroupPresenterContentMargin", new Thickness(0, 16, 0, 16));

            // ComboBox
            UpdateAppResource("ComboBoxMinHeight", 32);
            UpdateAppResource("ComboBoxPadding", new Thickness(12, 5, 0, 7));
        }

        // Toggle between the themes to force reload the resource styles.
        Api.Core.Theme.ApplicationTheme backupTheme = ActualAppTheme;
        ActualAppTheme = Api.Core.Theme.ApplicationTheme.Dark;
        ThemeChanged?.Invoke(this, EventArgs.Empty);

        ActualAppTheme = Api.Core.Theme.ApplicationTheme.Light;
        ThemeChanged?.Invoke(this, EventArgs.Empty);

        ActualAppTheme = backupTheme;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private static void UpdateAppResource(string resourceName, object value)
    {
        App.Current.Resources[resourceName] = value;
    }
}
