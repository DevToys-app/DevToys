using System.Windows;
using DevToys.Api;
using DevToys.Blazor.Components;
using DevToys.Core.Settings;
using Microsoft.Win32;

namespace DevToys.Windows.Core;

[Export(typeof(IThemeListener))]
internal sealed class ThemeListener : IThemeListener
{
    private const string WindowsThemeRegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string WindowsThemeRegistryValueName = "AppsUseLightTheme";

    private readonly ISettingsProvider _settingsProvider;

    private Window? _window;

    [ImportingConstructor]
    public ThemeListener(ISettingsProvider settingsProvider)
    {
        // Listen for app settings
        _settingsProvider = settingsProvider;
        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

        // Listen for operating system settings.
        SystemEvents.UserPreferenceChanged += SystemEventsUserPreferenceChanged;

        UpdateSystemSettingsAndApplyTheme();

        IsCompactMode = GetBestValueForCompactMode();
    }

    public AvailableApplicationTheme CurrentSystemTheme { get; private set; }

    public AvailableApplicationTheme CurrentAppTheme => _settingsProvider.GetSetting(DevToys.Core.Settings.PredefinedSettings.Theme);

    public ApplicationTheme ActualAppTheme { get; private set; }

    public bool IsHighContrast { get; private set; }

    public bool IsCompactMode { get; private set; }

    public bool UserIsCompactModePreference => _settingsProvider.GetSetting(PredefinedSettings.CompactMode);

    public bool UseLessAnimations => !SystemParameters.ClientAreaAnimation;

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

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void SetWindow(Window window)
    {
        Guard.IsNull(_window);
        Guard.IsNotNull(window);
        _window = window;

        _window.SizeChanged += Window_SizeChanged;
        UpdateCompactModeBasedOnWindowSize();
    }

    /// <summary>
    /// Evaluates whether the theme should automatically be updated or not, based on app and operating system settings.
    /// </summary>
    private void UpdateSystemSettingsAndApplyTheme()
    {
        IsHighContrast = SystemParameters.HighContrast;
        CurrentSystemTheme = GetCurrentSystemTheme();

        ApplyDesiredColorTheme();
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (string.Equals(DevToys.Core.Settings.PredefinedSettings.Theme.Name, e.SettingName, StringComparison.Ordinal))
        {
            ApplyDesiredColorTheme();
        }
        else if (string.Equals(PredefinedSettings.CompactMode.Name, e.SettingName, StringComparison.Ordinal))
        {
            IsCompactMode = GetBestValueForCompactMode();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateCompactModeBasedOnWindowSize();
    }

    private void SystemEventsUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General)
        {
            return;
        }

        UpdateSystemSettingsAndApplyTheme();
    }

    private void UpdateCompactModeBasedOnWindowSize()
    {
        bool newIsCompactMode = GetBestValueForCompactMode();
        if (newIsCompactMode != IsCompactMode)
        {
            IsCompactMode = newIsCompactMode;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool GetBestValueForCompactMode()
    {
        if (UserIsCompactModePreference)
        {
            return true;
        }

        if (_window is not null)
        {
            return _window.Width <= NavBarThresholds.NavBarWidthSidebarCollapseThreshold;
        }

        return false;
    }

    private static AvailableApplicationTheme GetCurrentSystemTheme()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(WindowsThemeRegistryKeyPath);
        object? registryValueObject = key?.GetValue(WindowsThemeRegistryValueName);

        if (registryValueObject == null)
        {
            return AvailableApplicationTheme.Light;
        }

        int registryValue = (int)registryValueObject;

        return registryValue > 0 ? AvailableApplicationTheme.Light : AvailableApplicationTheme.Dark;
    }
}
