#nullable enable

using System;
using System.Composition;
using DevToys.Api.Core.Settings;
using DevToys.Api.Core.Theme;
using DevToys.Core.Settings;
using DevToys.Core.Threading;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace DevToys.Core.Theme
{
    [Export(typeof(IThemeListener))]
    [Shared]
    internal sealed class ThemeListener : IThemeListener
    {
        private readonly AccessibilitySettings _accessible = new();
        private readonly UISettings _uiSettings = new();
        private readonly ISettingsProvider _settingsProvider;

        public AppTheme CurrentSystemTheme { get; private set; }

        public AppTheme CurrentAppTheme => _settingsProvider.GetSetting(PredefinedSettings.Theme);

        public ApplicationTheme ActualAppTheme { get; private set; }

        public bool IsHighContrast { get; private set; }

        public event EventHandler? ThemeChanged;

        [ImportingConstructor]
        public ThemeListener(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;

            _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

            CurrentSystemTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;
            IsHighContrast = _accessible.HighContrast;

            _accessible.HighContrastChanged += Accessible_HighContrastChanged;
            _uiSettings.ColorValuesChanged += UiSettings_ColorValuesChanged;

            if (Window.Current != null)
            {
                Window.Current.CoreWindow.Activated += CoreWindow_Activated;
            }
        }

        public void ApplyDesiredColorTheme()
        {
            AppTheme theme = CurrentAppTheme;

            if (theme == AppTheme.Default)
            {
                theme = CurrentSystemTheme;
            }

            // Set theme for window root.
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = theme == AppTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
                ActualAppTheme = theme == AppTheme.Light ? ApplicationTheme.Light : ApplicationTheme.Dark;
            }
        }

        private void Accessible_HighContrastChanged(AccessibilitySettings sender, object args)
        {
            ThreadHelper.RunOnUIThreadAsync(UpdateProperties).Forget();
        }

        private void UiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            // Note: This can get called multiple times during HighContrast switch, do we care?
            ThreadHelper.RunOnUIThreadAsync(
                () =>
                {
                    // TODO: This doesn't stop the multiple calls if we're in our faked 'White' HighContrast Mode below.
                    AppTheme currentAppTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;
                    if (CurrentSystemTheme != currentAppTheme || IsHighContrast != _accessible.HighContrast)
                    {
                        UpdateProperties();
                    }
                }).Forget();
        }

        private void CoreWindow_Activated(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.WindowActivatedEventArgs args)
        {
            AppTheme currentAppTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;
            if (CurrentSystemTheme != currentAppTheme || IsHighContrast != _accessible.HighContrast)
            {
                UpdateProperties();
            }
        }

        private void SettingsProvider_SettingChanged(object sender, SettingChangedEventArgs e)
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
            ThreadHelper.ThrowIfNotOnUIThread();

            // TODO: Not sure if HighContrastScheme names are localized?
            if (_accessible.HighContrast && _accessible.HighContrastScheme.IndexOf("white", StringComparison.OrdinalIgnoreCase) != -1)
            {
                IsHighContrast = false;
                CurrentSystemTheme = AppTheme.Light;
            }
            else
            {
                // Otherwise, we just set to what's in the system as we'd expect.
                IsHighContrast = _accessible.HighContrast;
                AppTheme currentAppTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;
                CurrentSystemTheme = currentAppTheme;
            }

            ApplyDesiredColorTheme();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}