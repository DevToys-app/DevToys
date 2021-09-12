#nullable enable

using DevTools.Core.Settings;
using DevTools.Core.Theme;
using DevTools.Core.Threading;
using System;
using System.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace DevTools.Core.Impl.Theme
{
    [Export(typeof(IThemeListener))]
    [Shared]
    internal sealed class ThemeListener : IThemeListener
    {
        private readonly AccessibilitySettings _accessible = new();
        private readonly UISettings _uiSettings = new();
        private readonly IThread _thread;
        private readonly ISettingsProvider _settingsProvider;

        public AppTheme CurrentSystemTheme { get; private set; }

        public AppTheme CurrentAppTheme => _settingsProvider.GetSetting(PredefinedSettings.Theme);

        public bool IsHighContrast { get; private set; }

        public event EventHandler? ThemeChanged;

        [ImportingConstructor]
        public ThemeListener(IThread thread, ISettingsProvider settingsProvider)
        {
            _thread = thread;
            _settingsProvider = settingsProvider;

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
            }
        }

        private void Accessible_HighContrastChanged(AccessibilitySettings sender, object args)
        {
            _thread.RunOnUIThreadAsync(UpdateProperties).Forget();
        }

        private void UiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            // Note: This can get called multiple times during HighContrast switch, do we care?
            _thread.RunOnUIThreadAsync(
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

        /// <summary>
        /// Set our current properties and fire a change notification.
        /// </summary>
        private void UpdateProperties()
        {
            _thread.ThrowIfNotOnUIThread();

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