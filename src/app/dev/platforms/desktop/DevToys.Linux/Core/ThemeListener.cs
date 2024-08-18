using DevToys.Api;
using DevToys.Core.Settings;
using DevToys.Blazor.Components;
using DevToys.Blazor.Core.Services;
using Gio;
using GLib;
using Microsoft.Extensions.Logging;
using Object = GObject.Object;
using static GObject.Object;

namespace DevToys.Linux.Core;

[Export(typeof(IThemeListener))]
internal sealed partial class ThemeListener : IThemeListener
{
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly Gtk.Settings _gtkSettings;

    private Gtk.Window? _mainWindow;
    private bool _ignoreOperatingSystemSettingChanged;

    [ImportingConstructor]
    public ThemeListener(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();

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
                _gtkSettings.GtkApplicationPreferDarkTheme = true;
            }
            else
            {
                ActualAppTheme = ApplicationTheme.Light;
                _gtkSettings.GtkApplicationPreferDarkTheme = false;
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
        try
        {
            var bus = DBusConnection.Get(BusType.Session);
            using var parameters = Variant.NewTuple([
                Variant.NewString("org.freedesktop.appearance"), Variant.NewString("color-scheme")
            ]);

            using Variant ret = bus.CallSync(
                busName: "org.freedesktop.portal.Desktop",
                objectPath: "/org/freedesktop/portal/desktop",
                interfaceName: "org.freedesktop.portal.Settings",
                methodName: "Read",
                parameters: parameters,
                replyType: VariantType.New("(v)"),
                flags: DBusCallFlags.None,
                timeoutMsec: 2000,
                cancellable: null
            );

            uint userThemePreference = ret.GetChildValue(0).GetVariant().GetVariant().GetUint32();

            return userThemePreference switch
            {
                1 => AvailableApplicationTheme.Dark,
                2 => AvailableApplicationTheme.Light,
                _ => FallBack()
            };
        }
        catch (Exception ex)
        {
            LogGetLinuxThemeFailed(ex);
            return FallBack();
        }

        AvailableApplicationTheme FallBack()
        {
            return _gtkSettings.GtkThemeName?.Contains("Dark", StringComparison.OrdinalIgnoreCase) ?? false
                ? AvailableApplicationTheme.Dark
                : AvailableApplicationTheme.Light;
        }
    }

    [LoggerMessage(0, LogLevel.Error, "Failed to detect Linux theme.")]
    partial void LogGetLinuxThemeFailed(Exception ex);
}
