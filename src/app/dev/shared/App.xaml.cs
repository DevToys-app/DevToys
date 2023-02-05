using DevToys.Api;
using DevToys.Api.Core.Threading;
using DevToys.Core.Mef;
using DevToys.Core.Settings;
using DevToys.UI;
using DevToys.UI.Framework.Controls;
using DevToys.UI.Views;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Uno.Extensions;
using Uno.Logging;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

namespace DevToys;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public sealed partial class App : Application
{
    // Always keep an instance of MefComposer alive so GC doesn't purge it.
    private readonly Task<MefComposer> _mefComposer;

    private readonly Task<IMefProvider> _mefProvider;
    private readonly AsyncLazy<ISettingsProvider> _settingsProvider;

    private MainWindow? _mainWindow;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeLogging();

        this.UnhandledException += OnUnhandledException;

        this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
        this.Suspending += OnSuspending;
#endif

        _mefComposer = Task.Run(() => new MefComposer());
        _mefProvider = Task.Run(async () =>
        {
            return (await _mefComposer).Provider;
        });
        _settingsProvider = new AsyncLazy<ISettingsProvider>(async () => (await _mefProvider).Import<ISettingsProvider>());
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
#if DEBUG
        if (Debugger.IsAttached)
        {
            // this.DebugSettings.EnableFrameRateCounter = true;
        }
#endif

        // Set the user-defined language.
        string? languageIdentifier = (await _settingsProvider.GetValueAsync()).GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l => string.Equals(l.InternalName, languageIdentifier))
            ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);

#if __WINDOWS__
        // On Windows 10 version 1607 or later, this code signals that this app wants to participate in prelaunch
        CoreApplication.EnablePrelaunch(true);
#endif

#if __WINDOWS__
        _mainWindow = new MainWindow(new BackdropWindow());
        _mainWindow.Show();
#elif __MACCATALYST__
        // Important! Keep the full name `Microsoft.UI.Xaml.Window.Current` otherwise the Mac app won't build.
        // See https://blog.mzikmund.com/2020/04/resolving-uno-platform-uiwindow-does-not-contain-a-definition-for-current-issue/
        _mainWindow = new MainWindow(new BackdropWindow(Microsoft.UI.Xaml.Window.Current));
#else
        _mainWindow = new MainWindow(new BackdropWindow(Window.Current));
#endif

        Windows.ApplicationModel.Activation.LaunchActivatedEventArgs uwpArgs = args.UWPLaunchActivatedEventArgs;

#if !(NET6_0_OR_GREATER && WINDOWS)
        if (uwpArgs.PrelaunchActivated == false)
#endif
        {
            // Ensure the current window is active
            _mainWindow.Activate();
        }
    }

    /// <summary>
    /// Invoked when application execution is being suspended.  Application state is saved
    /// without knowing whether the application will be terminated or resumed with the contents
    /// of memory still intact.
    /// </summary>
    /// <param name="sender">The source of the suspend request.</param>
    /// <param name="e">Details about the suspend request.</param>
#if __WINDOWS__
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
#endif
    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
        SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
        //TODO: Save application state and stop any background activity
        deferral.Complete();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ILogger logger = this.Log();
        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.Error($"Unhandled exception !!!", e.Exception);
        }

        // TODO:
        // Maybe we need to dispose all tools in case if they have on-going work so they have a chance to finish correctly?
        // Also, use IMarketingService to NotifyAppEncounteredAProblemAsync ?
    }

    /// <summary>
    /// Configures global Uno Platform logging
    /// </summary>
    private static void InitializeLogging()
    {
#if DEBUG
        // Logging is disabled by default for release builds, as it incurs a significant
        // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
        // is a concern for your application, keep this disabled. If you're running on web or 
        // desktop targets, you can use url or command line parameters to enable it.
        //
        // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

        ILoggerFactory factory = LoggerFactory.Create((ILoggingBuilder builder) =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __MACCATALYST__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
            builder.AddDebug();
#else
            builder.AddConsole();
            builder.AddDebug();
#endif

            // Exclude logs below this level
            builder.SetMinimumLevel(LogLevel.Information);

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);

            if (Debugger.IsAttached)
            {
                builder.SetMinimumLevel(LogLevel.Debug);

                // Generic Xaml events
                builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug);
                builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug);
                builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug);
                builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug);
                builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace);

                // Layouter specific messages
                builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug);
                builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug);
                builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug);

                builder.AddFilter("Windows.Storage", LogLevel.Debug);

                // Binding related messages
                builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug);
                builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug);

                // Binder memory references tracking
                builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug);

                // RemoteControl and HotReload related
                builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug);
            }
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
