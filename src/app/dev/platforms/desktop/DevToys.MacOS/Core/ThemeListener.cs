using DevToys.Api;
using DevToys.Blazor.Components;
using DevToys.Core.Settings;
using DevToys.MacOS.Core.Helpers;
using DevToys.MacOS.Views;

namespace DevToys.MacOS.Core;

[Export(typeof(IThemeListener))]
internal sealed class ThemeListener : IThemeListener, IDisposable
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly SystemThemeObserver _systemThemeObserver = new();

    private bool _ignoreSystemThemeChangeNotification;

    [ImportingConstructor]
    public ThemeListener(ISettingsProvider settingsProvider)
    {
        // Listen for app settings
        _settingsProvider = settingsProvider;
        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

        // Listen for system theme changes.
        _systemThemeObserver.SystemThemeChanged += OnSystemThemeChanged;

        // Is it safe because we're already on the UI Thread and this method asynchronously switch to it?
        // We really need to determine ActualAppTheme before the end of the constructor. Alternatively,
        // we can make ActualAppTheme asynchronous or share a flag indicating whether it's up to date or not.
        UpdateSystemSettingsAndApplyThemeAsync()
            .CompleteOnCurrentThread();

        IsCompactMode = GetBestValueForCompactMode();
        MainWindow.Instance.WillStartLiveResize += OnMainWindowSizeChanged;
        MainWindow.Instance.DidResize += OnMainWindowSizeChanged;
        MainWindow.Instance.DidEndLiveResize += OnMainWindowSizeChanged;
    }

    public AvailableApplicationTheme CurrentSystemTheme { get; private set; }

    public AvailableApplicationTheme CurrentAppTheme => _settingsProvider.GetSetting(PredefinedSettings.Theme);

    public ApplicationTheme ActualAppTheme { get; private set; }

    public bool IsHighContrast { get; private set; }

    public bool IsCompactMode { get; private set; }

    public bool UserIsCompactModePreference => _settingsProvider.GetSetting(PredefinedSettings.CompactMode);

    public bool UseLessAnimations => true;

    public event EventHandler? ThemeChanged;

    public void Dispose()
    {
        _systemThemeObserver.Dispose();
    }

    public void ApplyDesiredColorTheme()
    {
        ThreadHelper.RunOnUIThreadAsync(async () =>
        {
            AvailableApplicationTheme theme = CurrentAppTheme;

            if (theme == AvailableApplicationTheme.Default)
            {
                // Setting NSApplication.SharedApplication.Appearance to null makes the app adopting the theme of the system.
                _ignoreSystemThemeChangeNotification = true;
                NSApplication.SharedApplication.Appearance = null;
                CurrentSystemTheme = await GetCurrentSystemThemeAsync().ConfigureAwait(true);

                theme = CurrentSystemTheme;

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
                    NSApplication.SharedApplication.Appearance = NSAppearance.GetAppearance(NSAppearance.NameDarkAqua);
                }
                else
                {
                    ActualAppTheme = ApplicationTheme.Light;
                    NSApplication.SharedApplication.Appearance = NSAppearance.GetAppearance(NSAppearance.NameAqua);
                }
            }

            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }).Forget();
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

    private void OnMainWindowSizeChanged(object? sender, EventArgs e)
    {
        UpdateCompactModeBasedOnWindowSize();
    }

    private void OnSystemThemeChanged(object? sender, EventArgs e)
    {
        if (_ignoreSystemThemeChangeNotification)
        {
            _ignoreSystemThemeChangeNotification = false;
        }
        else
        {
            UpdateSystemSettingsAndApplyThemeAsync().Forget();
        }
    }

    /// <summary>
    /// Evaluates whether the theme should automatically be updated or not, based on app and operating system settings.
    /// </summary>
    private async Task UpdateSystemSettingsAndApplyThemeAsync()
    {
        IsHighContrast = false; // TODO: Detect high contrast
        CurrentSystemTheme = await GetCurrentSystemThemeAsync();

        ApplyDesiredColorTheme();
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

        return MainWindow.Instance.Frame.Width <= NavBarThresholds.NavBarWidthSidebarCollapseThreshold;
    }

    private static Task<AvailableApplicationTheme> GetCurrentSystemThemeAsync()
    {
        return ThreadHelper.RunOnUIThreadAsync(() =>
        {
            NSAppearance newAppearance = NSApplication.SharedApplication.EffectiveAppearance;
            if (newAppearance.FindBestMatch(new string[] { NSAppearance.NameAqua, NSAppearance.NameDarkAqua }) == NSAppearance.NameDarkAqua)
            {
                return AvailableApplicationTheme.Dark;
            }

            return AvailableApplicationTheme.Light;
        });
    }

    private sealed class SystemThemeObserver : NSObject
    {
        internal SystemThemeObserver()
        {
            // Observe changes in effectiveAppearance
            NSApplication.SharedApplication.AddObserver(
                this,
                new NSString("effectiveAppearance"),
                NSKeyValueObservingOptions.New,
                IntPtr.Zero);
        }

        internal EventHandler? SystemThemeChanged;

        // Respond to changes in effectiveAppearance
        [Foundation.Export("observeValueForKeyPath:ofObject:change:context:")]
        public void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == "effectiveAppearance")
            {
                SystemThemeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            NSApplication.SharedApplication.RemoveObserver(this, new NSString("effectiveAppearance"));
            base.Dispose(disposing);
        }
    }
}
