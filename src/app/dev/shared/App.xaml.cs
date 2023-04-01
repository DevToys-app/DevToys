using DevToys.Api;
using DevToys.Api.Core;
using DevToys.Api.Core.Theme;
using DevToys.Business.ViewModels;
using DevToys.Core.Logging;
using DevToys.Core.Mef;
using DevToys.Core.Settings;
using DevToys.Tools;
using DevToys.UI;
using DevToys.UI.Framework.Controls;
using DevToys.UI.Framework.Helpers;
using DevToys.UI.Views;
using DevToys.Wasdk.Core.FileStorage;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Uno.Extensions;
using Uno.Logging;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;
using PredefinedSettings = DevToys.Core.Settings.PredefinedSettings;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

namespace DevToys;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public sealed partial class App : Application
{
    // Always keep an instance of MefComposer alive so GC doesn't purge it.
    private readonly MefComposer _mefComposer;

    private readonly IMefProvider _mefProvider;
    private readonly Lazy<ISettingsProvider> _settingsProvider;
    private readonly Lazy<IThemeListener> _themeListener;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        DateTime startTime = DateTime.Now;
        _loggerFactory = InitializeLogging();
        _logger = this.Log();
        LogLoggerInitialization((DateTime.Now - startTime).TotalMilliseconds);
        LogAppStarting();

        this.UnhandledException += OnUnhandledException;

#if HAS_UNO || NETFX_CORE
        this.Suspending += OnSuspending;
#endif

        _mefComposer
            = new MefComposer(
                new[]
                {
                    typeof(DevToysToolsResourceManagerAssemblyIdentifier).Assembly,
                    typeof(MainWindow).Assembly,
                    typeof(MainWindowViewModel).Assembly,
                    typeof(MonacoEditor.CodeEditor).Assembly
                });

        _mefProvider = _mefComposer.Provider;

        _settingsProvider = new Lazy<ISettingsProvider>(_mefProvider.Import<ISettingsProvider>);
        _themeListener = new Lazy<IThemeListener>(_mefProvider.Import<IThemeListener>());
    }

    internal MainWindow? MainWindow { get; private set; }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
#if DEBUG
        if (Debugger.IsAttached)
        {
            // this.DebugSettings.EnableFrameRateCounter = true;
        }
#endif

        // Set the user-defined language.
        string? languageIdentifier = _settingsProvider.Value.GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l => string.Equals(l.InternalName, languageIdentifier))
            ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);

        IThemeListener themeListener = _themeListener.Value;

        Parts.MefProvider = _mefProvider;

        SetDefaultTextEditorFont();

#if __WINDOWS__
        // On Windows 10 version 1607 or later, this code signals that this app wants to participate in prelaunch
        CoreApplication.EnablePrelaunch(true);
#endif

#if __WINDOWS__
        MainWindow = new MainWindow(new BackdropWindow(), themeListener, _mefProvider);
#elif __MACCATALYST__
        // Important! Keep the full name `Microsoft.UI.Xaml.Window.Current` otherwise the Mac app won't build.
        // See https://blog.mzikmund.com/2020/04/resolving-uno-platform-uiwindow-does-not-contain-a-definition-for-current-issue/
        MainWindow = new MainWindow(new BackdropWindow(Microsoft.UI.Xaml.Window.Current), themeListener, _mefProvider);
#else
        MainWindow = new MainWindow(new BackdropWindow(Window.Current), themeListener, _mefProvider);
#endif

        // Apply the app color theme.
        themeListener.ApplyDesiredColorTheme();

        MainWindow.Activated += MainWindow_Activated;
        MainWindow.Closed += MainWindow_Closed;

        MainWindow.Show();

        Windows.ApplicationModel.Activation.LaunchActivatedEventArgs uwpArgs = args.UWPLaunchActivatedEventArgs;

#if !(NET6_0_OR_GREATER && WINDOWS)
        if (uwpArgs.PrelaunchActivated == false)
#endif
        {
            // Ensure the current window is active
            MainWindow.Activate();
        }
    }

    private void MainWindow_Closed(BackdropWindow sender, EventArgs args)
    {
        LogAppShuttingDown();
        _loggerFactory.Dispose();

        Guard.IsNotNull(MainWindow);
        MainWindow.Activated -= MainWindow_Activated;
        MainWindow.Closed -= MainWindow_Closed;
    }

    private void MainWindow_Activated(BackdropWindow sender, WindowActivatedEventArgs args)
    {
        _themeListener.Value.UpdateThemeIfNeeded();
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
        LogUnhandledException(e.Exception);
        _loggerFactory.Dispose();

        // TODO:
        // Maybe we need to dispose all tools in case if they have on-going work so they have a chance to finish correctly?
        // Also, use IMarketingService to NotifyAppEncounteredAProblemAsync ?
    }

    private void SetDefaultTextEditorFont()
    {
        string? currentFontName = _settingsProvider.Value.GetSetting(Api.PredefinedSettings.TextEditorFont);
        if (string.IsNullOrEmpty(currentFontName))
        {
            // TODO: https://github.com/baskren/P42.Utils/blob/8e97ecdc7be8e792b63c2703a52d951800f4fb31/P42.Utils.Uno/AvailableFonts/AvailableFonts.cs
        }
    }

    /// <summary>
    /// Configures global Uno Platform logging
    /// </summary>
    private static ILoggerFactory InitializeLogging()
    {
        // Logging is disabled by default for release builds, as it incurs a significant
        // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
        // is a concern for your application, keep this disabled. If you're running on web or 
        // desktop targets, you can use url or command line parameters to enable it.
        //
        // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

        ILoggerFactory factory = LoggerFactory.Create((ILoggingBuilder builder) =>
        {
#if DEBUG
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
#endif

            // To save logs on local hard drive.
            builder.AddFile(new FileStorage());

            // Exclude logs below this level
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Trace);
#else
            builder.SetMinimumLevel(LogLevel.Information);
#endif

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);

#if DEBUG
            if (Debugger.IsAttached)
            {
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
#endif
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif

        return factory;
    }

    [LoggerMessage(0, LogLevel.Information, "App is starting...")]
    partial void LogAppStarting();

    [LoggerMessage(1, LogLevel.Information, "App is shutting down...")]
    partial void LogAppShuttingDown();

    [LoggerMessage(2, LogLevel.Critical, "Unhandled exception !!!    (╯°□°）╯︵ ┻━┻")]
    partial void LogUnhandledException(Exception exception);

    [LoggerMessage(3, LogLevel.Information, "Logger initialized in {duration} ms")]
    partial void LogLoggerInitialization(double duration);
}
