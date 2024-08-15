using DevToys.Api;
using DevToys.Core.Settings;
using DevToys.Blazor.Components;
using DevToys.Blazor.Core.Services;
using Object = GObject.Object;
using static GObject.Object;

namespace DevToys.Linux.Core;

[Export(typeof(IThemeListener))]
internal sealed class ThemeListener : IThemeListener
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly Gtk.Settings _gtkSettings;

    private Gtk.Window? _mainWindow;
    private bool _ignoreOperatingSystemSettingChanged;

    [ImportingConstructor]
    public ThemeListener(ISettingsProvider settingsProvider)
    {
        // Listen for app settings
        _settingsProvider = settingsProvider;
        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

        // Listen for operating system settings.
        _gtkSettings = Gtk.Settings.GetDefault()!;
        _gtkSettings.OnNotify += System_RequestedThemeChanged;

        UpdateSystemSettingsAndApplyTheme();

        IsCompactMode = GetBestValueForCompactMode();
    }

    public AvailableApplicationTheme CurrentSystemTheme { get; private set; }

    public AvailableApplicationTheme CurrentAppTheme => _settingsProvider.GetSetting(PredefinedSettings.Theme);

    public ApplicationTheme ActualAppTheme { get; private set; }

    public bool IsHighContrast { get; private set; }

    public bool IsCompactMode { get; private set; }

    public bool UserIsCompactModePreference => _settingsProvider.GetSetting(PredefinedSettings.CompactMode);

    public bool UseLessAnimations => true;

    public event EventHandler? ThemeChanged;

    public void ApplyDesiredColorTheme()
    {
        AvailableApplicationTheme theme = CurrentAppTheme;

        if (theme == AvailableApplicationTheme.Default)
        {
            theme = CurrentSystemTheme;
        }

        // Set theme for window root.
        if (_gtkSettings.GtkThemeName is not null)
        {
            _ignoreOperatingSystemSettingChanged = true;
            if (theme == AvailableApplicationTheme.Dark)
            {
                ActualAppTheme = ApplicationTheme.Dark;
            }
            else
            {
                ActualAppTheme = ApplicationTheme.Light;
            }

            _ignoreOperatingSystemSettingChanged = false;
        }

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void SetMainWindow(Gtk.Window window, IWindowService windowService)
    {
        _mainWindow = window;
        windowService.WindowSizeChanged += WindowService_WindowSizeChanged;
    }

    private void WindowService_WindowSizeChanged(object? sender, EventArgs e)
    {
        UpdateCompactModeBasedOnWindowSize();
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (string.Equals(PredefinedSettings.Theme.Name, e.SettingName, StringComparison.Ordinal))
        {
            ApplyDesiredColorTheme();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (string.Equals(PredefinedSettings.CompactMode.Name, e.SettingName, StringComparison.Ordinal))
        {
            IsCompactMode = GetBestValueForCompactMode();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void System_RequestedThemeChanged(Object? sender, NotifySignalArgs e)
    {
        if (!_ignoreOperatingSystemSettingChanged)
        {
            UpdateSystemSettingsAndApplyTheme();
        }
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

        if (_mainWindow is not null)
        {
            return _mainWindow.GetWidth() <= NavBarThresholds.NavBarWidthSidebarCollapseThreshold;
        }

        return false;
    }

    private void UpdateSystemSettingsAndApplyTheme()
    {
        IsHighContrast
            = _gtkSettings.GtkThemeName is not null
              && _gtkSettings.GtkThemeName.Contains("high", StringComparison.OrdinalIgnoreCase)
              && _gtkSettings.GtkThemeName.Contains("contrast", StringComparison.OrdinalIgnoreCase);
        CurrentSystemTheme = GetCurrentSystemTheme();

        ApplyDesiredColorTheme();
    }

    private AvailableApplicationTheme GetCurrentSystemTheme()
    {
        return _gtkSettings.GtkApplicationPreferDarkTheme || (_gtkSettings.GtkThemeName?.Contains("Dark", StringComparison.OrdinalIgnoreCase) ?? false)
            ? AvailableApplicationTheme.Dark
            : AvailableApplicationTheme.Light;
    }
}
