#nullable enable

using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

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
        private readonly UISettings _uiSettings = new UISettings();

        public string CurrentThemeName { get { return CurrentTheme.ToString(); } } // For Web Retrieval

        public string AccentColorHtmlHex { get; private set; }

        public ApplicationTheme CurrentTheme { get; set; }

        public bool IsHighContrast { get; set; }

        public event ThemeChangedEvent? ThemeChanged;

        public ThemeListener()
        {
            AccentColorHtmlHex = ToHtmlHex(((SolidColorBrush)Application.Current.Resources["TextControlSelectionHighlightColor"]).Color);
            CurrentTheme = Application.Current.RequestedTheme;
            IsHighContrast = _accessible.HighContrast;

            _accessible.HighContrastChanged += Accessible_HighContrastChanged;
            _uiSettings.ColorValuesChanged += UiSettings_ColorValuesChanged;

            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                CurrentTheme = frameworkElement.ActualTheme == ElementTheme.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
                frameworkElement.ActualThemeChanged += Window_ActualThemeChanged;
            }
        }

        ~ThemeListener()
        {
            _accessible.HighContrastChanged -= Accessible_HighContrastChanged;
        }

        private void Window_ActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateProperties();
        }

        private async void UiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            // Getting called off thread, so we need to dispatch to request value.
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                UpdateProperties();
            });
        }

        private void Accessible_HighContrastChanged(AccessibilitySettings sender, object args)
        {
            UpdateProperties();
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
                CurrentTheme = ((FrameworkElement)Window.Current.Content).ActualTheme == ElementTheme.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
            }

            AccentColorHtmlHex = ToHtmlHex(((SolidColorBrush)Application.Current.Resources["TextControlSelectionHighlightColor"]).Color);

            ThemeChanged?.Invoke(this);
        }

        public static string ToHtmlHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
        }
    }
}
