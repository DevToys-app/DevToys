#nullable enable

using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace DevToys.MonacoEditor.Helpers
{
    public delegate void ThemeChangedEvent(ThemeListener sender);

    /// <summary>
    /// Class which listens for changes to Application Theme or High Contrast Modes 
    /// and Signals an Event when they occur.
    /// </summary>
    [AllowForWeb]
    public sealed class ThemeListener
    {
        private readonly AccessibilitySettings _accessible = new AccessibilitySettings();
        private readonly UISettings _settings = new UISettings();

        public string CurrentThemeName { get { return CurrentTheme.ToString(); } } // For Web Retrieval

        public ApplicationTheme CurrentTheme { get; set; }

        public bool IsHighContrast { get; set; }

        public event ThemeChangedEvent? ThemeChanged;

        public ThemeListener()
        {
            CurrentTheme = Application.Current.RequestedTheme;
            IsHighContrast = _accessible.HighContrast;

            _accessible.HighContrastChanged += _accessible_HighContrastChanged;
            _settings.ColorValuesChanged += _settings_ColorValuesChanged;

            // Fallback in case either of the above fail, we'll check when we get activated next.
            Window.Current.CoreWindow.Activated += CoreWindow_Activated;
        }

        ~ThemeListener()
        {
            _accessible.HighContrastChanged -= _accessible_HighContrastChanged;
            _settings.ColorValuesChanged -= _settings_ColorValuesChanged;

            Window.Current.CoreWindow.Activated -= CoreWindow_Activated;
        }

        private void _accessible_HighContrastChanged(AccessibilitySettings sender, object args)
        {
#if DEBUG
            Debug.WriteLine("HighContrast Changed");
#endif

            UpdateProperties();
        }

        // Note: This can get called multiple times during HighContrast switch, do we care?
        private async void _settings_ColorValuesChanged(UISettings sender, object args)
        {
            // Getting called off thread, so we need to dispatch to request value.
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // TODO: This doesn't stop the multiple calls if we're in our faked 'White' HighContrast Mode below.
                if (CurrentTheme != Application.Current.RequestedTheme ||
                    IsHighContrast != _accessible.HighContrast)
                {
#if DEBUG
                    Debug.WriteLine("Color Values Changed");
#endif

                    UpdateProperties();
                }
            });
        }

        private void CoreWindow_Activated(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.WindowActivatedEventArgs args)
        {
            if (CurrentTheme != Application.Current.RequestedTheme ||
                IsHighContrast != _accessible.HighContrast)
            {
#if DEBUG
                Debug.WriteLine("CoreWindow Activated Changed");
#endif

                UpdateProperties();
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
                // If our HighContrastScheme is ON & a lighter one, then we should remain in 'Light' theme mode for Monaco Themes Perspective
                IsHighContrast = false;
                CurrentTheme = ApplicationTheme.Light;
            }
            else
            {
                // Otherwise, we just set to what's in the system as we'd expect.
                IsHighContrast = _accessible.HighContrast;
                CurrentTheme = Application.Current.RequestedTheme;
            }

            ThemeChanged?.Invoke(this);
        }
    }
}
