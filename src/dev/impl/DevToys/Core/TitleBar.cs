#nullable enable

using System.ComponentModel;
using System.Composition;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Core.Theme;
using DevToys.Core.Settings;
using DevToys.Core.Threading;
using DevToys.Shared.Core;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace DevToys.Core
{
    [Export(typeof(ITitleBar))]
    [Shared]
    internal sealed class TitleBar : ITitleBar
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IThemeListener _themeListener;

        private double _systemOverlayRightInset;

        public double SystemOverlayRightInset
        {
            get => _systemOverlayRightInset;
            private set
            {
                _systemOverlayRightInset = value;
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SystemOverlayRightInset)));
                });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [ImportingConstructor]
        public TitleBar(ISettingsProvider settingsProvider, IThemeListener themeListener)
        {
            _settingsProvider = settingsProvider;
            _themeListener = themeListener;
        }

        public Task SetupTitleBarAsync()
        {
            return ThreadHelper.RunOnUIThreadAsync(() =>
            {
                SetupTitleBar(CoreApplication.GetCurrentView().TitleBar);
            });
        }

        private void SetupTitleBar(CoreApplicationViewTitleBar coreTitleBar)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Arguments.NotNull(coreTitleBar, nameof(coreTitleBar));

            Window.Current.SizeChanged += CurrentWindow_SizeChanged;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            SystemOverlayRightInset = coreTitleBar.SystemOverlayRightInset;

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            ApplyThemeForTitleBarButtons();
        }

        private void ApplyThemeForTitleBarButtons()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            AppTheme theme = _settingsProvider.GetSetting(PredefinedSettings.Theme);
            if (theme == AppTheme.Default)
            {
                theme = _themeListener.CurrentSystemTheme;
            }

            if (theme == AppTheme.Dark)
            {
                // Set active window colors
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 90, 90, 90);
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 120, 120, 120);

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Colors.Gray;
                titleBar.InactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Colors.Gray;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                titleBar.BackgroundColor = Color.FromArgb(255, 45, 45, 45);
            }
            else if (theme == AppTheme.Light)
            {
                // Set active window colors
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 180, 180, 180);
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 150, 150, 150);

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Colors.DimGray;
                titleBar.InactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Colors.DimGray;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                titleBar.BackgroundColor = Color.FromArgb(255, 210, 210, 210);
            }
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            SetupTitleBar(sender);
        }

        private void CurrentWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            SystemOverlayRightInset = CoreApplication.GetCurrentView().TitleBar.SystemOverlayRightInset;
        }
    }
}
