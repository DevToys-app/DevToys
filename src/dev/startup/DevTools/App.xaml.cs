#nullable enable

using DevTools.Core;
using DevTools.Core.Impl.Injection;
using DevTools.Core.Injection;
using DevTools.Core.Navigation;
using DevTools.Core.Settings;
using DevTools.Core.Theme;
using DevTools.Core.Threading;
using DevTools.Impl.Views;
using DevTools.Common;
using DevTools.Providers;
using System;
using System.Globalization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevTools
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application, IDisposable
    {
        private readonly MefComposer _mefComposer;
        private readonly ISettingsProvider _settingsProvider;
        private readonly Lazy<IThemeListener> _themeListener;

        private bool _isDisposed;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Set the language of the app.
            LanguageManager.Instance.SetCurrentCulture(CultureInfo.InstalledUICulture);

            // Initialize MEF
            _mefComposer
                = new MefComposer(
                    typeof(MefComposer).Assembly,
                    typeof(IToolProvider).Assembly,
                    typeof(Providers.Impl.Dummy).Assembly,
                    typeof(Impl.Dummy).Assembly);

            _settingsProvider = _mefComposer.ExportProvider.GetExport<ISettingsProvider>();
            _themeListener = new Lazy<IThemeListener>(() => _mefComposer.ExportProvider.GetExport<IThemeListener>());

            InitializeComponent();
            Suspending += OnSuspending;
        }

        ~App()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _mefComposer?.Dispose();
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = EnsureWindowIsInitialized();

            if (e.PrelaunchActivated == false)
            {
                // On Windows 10 version 1607 or later, this code signals that this app wants to participate in prelaunch
                CoreApplication.EnablePrelaunch(true);

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(
                        typeof(MainPage),
                        new NavigationParameter(
                            _mefComposer.ExportProvider.GetExport<IMefProvider>(),
                            e.Arguments));
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }

            // Setup the title bar.
            _mefComposer.ExportProvider.GetExport<ITitleBar>().SetupTitleBarAsync().Forget();
        }

        /// <summary>
        /// Invoked when the application is activated by some means other than normal launching.
        /// </summary>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            Frame rootFrame = EnsureWindowIsInitialized();

            if (args.Kind == ActivationKind.Protocol)
            {
                var eventArgs = (ProtocolActivatedEventArgs)args;
                rootFrame.Navigate(
                    typeof(MainPage),
                    new NavigationParameter(
                        _mefComposer.ExportProvider.GetExport<IMefProvider>(),
                        eventArgs.Uri.Query));

                // Ensure the current window is active
                Window.Current.Activate();

                // Setup the title bar.
                _mefComposer.ExportProvider.GetExport<ITitleBar>().SetupTitleBarAsync().Forget();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private Frame EnsureWindowIsInitialized()
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            applicationView.SetPreferredMinSize(new Windows.Foundation.Size(300, 200));

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            _themeListener.Value.ThemeChanged += ThemeListener_ThemeChanged;
            UpdateColorTheme();

            return rootFrame;
        }

        private void ThemeListener_ThemeChanged(object sender, EventArgs e)
        {
            UpdateColorTheme();
        }

        private void UpdateColorTheme()
        {
            AppTheme theme = _settingsProvider.GetSetting(PredefinedSettings.Theme);

            if (theme == AppTheme.Default)
            {
                theme = _themeListener.Value.CurrentTheme;
            }

            // Set theme for window root.
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = theme == AppTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
            }
        }
    }
}