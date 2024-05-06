using System.IO;
using System.Windows.Interop;
using DevToys.Api;
using DevToys.Blazor.BuiltInTools;
using DevToys.Blazor.BuiltInTools.ExtensionsManager;
using DevToys.Blazor.Core.Languages;
using DevToys.Blazor.Core.Services;
using DevToys.Business.Services;
using DevToys.Business.ViewModels;
using DevToys.Core;
using DevToys.Core.Logging;
using DevToys.Core.Mef;
using DevToys.Core.Tools;
using DevToys.Windows.Controls;
using DevToys.Windows.Core;
using DevToys.Windows.Core.Helpers;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PredefinedSettings = DevToys.Core.Settings.PredefinedSettings;

namespace DevToys.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MicaWindowWithOverlay
{
    private static MainWindow? mainWindowInstance;

    private readonly MefComposer _mefComposer;
    private readonly ServiceProvider _serviceProvider;
    private readonly DateTime _uiLoadingTime;
    private readonly ISettingsProvider _settingsProvider;
    private ILogger? _logger;

    public MainWindow()
    {
        mainWindowInstance = this;

        DateTime startTime = DateTime.Now;

        // Initialize services and logging.
        _serviceProvider = InitializeServices();

        // Listen for unhandled exceptions.
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Clear older temp files.
        FileHelper.ClearTempFiles(Constants.AppTempFolder);

        // Initialize extension installation folder, and uninstall extensions that are planned for being removed.
        string[] pluginFolders
            = new[]
            {
                Path.Combine(AppContext.BaseDirectory!, "Plugins"),
                Constants.PluginInstallationFolder
            };
        ExtensionInstallationManager.PreferredExtensionInstallationFolder = Constants.PluginInstallationFolder;
        ExtensionInstallationManager.ExtensionInstallationFolders = pluginFolders;
        ExtensionInstallationManager.UninstallExtensionsScheduledForRemoval();

        // Initialize MEF.
        _mefComposer
            = new MefComposer(
                assemblies: new[] {
                    typeof(MainWindowViewModel).Assembly,
                    typeof(DevToysBlazorResourceManagerAssemblyIdentifier).Assembly
                },
                pluginFolders);

        LogInitialization((DateTime.Now - startTime).TotalMilliseconds);
        LogAppStarting();

        _uiLoadingTime = DateTime.Now;
        _settingsProvider = _mefComposer.Provider.Import<ISettingsProvider>();

        // Set the user-defined language.
        string? languageIdentifier = _settingsProvider.GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l => string.Equals(l.InternalName, languageIdentifier))
            ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);

        // Load the UI.
        Resources.Add("services", _serviceProvider);
        InitializeComponent();

        _efficiencyModeService = _mefComposer.Provider.Import<EfficiencyModeService>();
        _themeListener = _mefComposer.Provider.Import<IThemeListener>();
        DataContext = _mefComposer.Provider.Import<TitleBarInfoProvider>();

        blazorWebView.BlazorWebViewInitializing += BlazorWebView_BlazorWebViewInitializing;
        blazorWebView.BlazorWebViewInitialized += BlazorWebView_BlazorWebViewInitialized;

        SetPositionAndSize();
    }

    private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        var windowService = (WindowService)_serviceProvider.GetService<IWindowService>()!;
        windowService.SetWindow(this);

        Guard.IsNotNull(_themeListener);
        ((ThemeListener)_themeListener).SetWindow(this);
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        SavePositionAndSize();

        // Dispose every disposable tool instance.
        _mefComposer.Provider.Import<GuiToolProvider>().DisposeTools();

        // Clear older temp files.
        FileHelper.ClearTempFiles(Constants.AppTempFolder);
    }

    private void BlazorWebView_BlazorWebViewInitializing(object? sender, BlazorWebViewInitializingEventArgs e)
    {
        // Set the web view transparent.
        blazorWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
    }

    private void BlazorWebView_BlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
    {
        LogWebViewVersion(blazorWebView.WebView.CoreWebView2.Environment.BrowserVersionString);

        if (!Debugger.IsAttached)
        {
            blazorWebView.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        }
        blazorWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        blazorWebView.WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
        blazorWebView.WebView.CoreWebView2.Settings.IsPinchZoomEnabled = false;
        blazorWebView.WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
        blazorWebView.WebView.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
        blazorWebView.WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
        blazorWebView.Focus();
        blazorWebView.WebView.Focus();
        LogUiLoadTime((DateTime.Now - _uiLoadingTime).TotalMilliseconds);

        InitializeLowPriorityServices();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Guard.IsNotNull(mainWindowInstance);
        mainWindowInstance.LogUnhandledException((Exception)e.ExceptionObject);
    }

    private ServiceProvider InitializeServices()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddWpfBlazorWebView();

#if DEBUG
        serviceCollection.AddBlazorWebViewDeveloperTools();
#endif

        serviceCollection.AddLogging((ILoggingBuilder builder) =>
        {
#if DEBUG
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);
#else
            builder.SetMinimumLevel(LogLevel.Information);
#endif

            // To save logs on local hard drive.
            builder.AddFile(new FileStorage());

            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
        });

        serviceCollection.AddSingleton(provider => _mefComposer.Provider);
        serviceCollection.AddSingleton<IWindowService, WindowService>();
        serviceCollection.AddScoped<DocumentEventService, DocumentEventService>();
        serviceCollection.AddScoped<PopoverService, PopoverService>();
        serviceCollection.AddScoped<ContextMenuService, ContextMenuService>();
        serviceCollection.AddScoped<GlobalDialogService, GlobalDialogService>();
        serviceCollection.AddScoped<UIDialogService, UIDialogService>();
        serviceCollection.AddScoped<FontService, FontService>();

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        LoggingExtensions.LoggerFactory = loggerFactory;
        _logger = typeof(MainWindow).Log();

        return serviceProvider;
    }

    private void InitializeLowPriorityServices()
    {
        // Start the Taskbar Jump List service after the web view loaded.
        _mefComposer.Provider.Import<TaskbarJumpListService>();

        // Treat command line arguments.
        _mefComposer.Provider.Import<CommandLineLauncherService>().HandleCommandLineArguments();
    }

    private void SetPositionAndSize()
    {
        var dpiHelper = new DpiHelper(this);
        double DPI_SCALE = dpiHelper.LogicalToDeviceUnitsScalingFactorX;
        var windowInteropHelper = new WindowInteropHelper(this);
        var screen = Screen.FromHandle(windowInteropHelper.Handle);

        SixLabors.ImageSharp.Rectangle? bounds = _settingsProvider.GetSetting(PredefinedSettings.MainWindowBounds);

        if (bounds is null)
        {
            int width = (int)(Math.Max(screen.WorkingArea.Width - 400, 1200) / DPI_SCALE);
            int height = (int)(Math.Max(screen.WorkingArea.Height - 200, 600) / DPI_SCALE);

            // Center the window on the screen.
            bounds
                = new(
                    x: (int)(((screen.WorkingArea.Width / DPI_SCALE) - width) / 2),
                    y: (int)(((screen.WorkingArea.Height / DPI_SCALE) - height) / 2),
                    width,
                    height);
        }

        Left = bounds.Value.X;
        Top = bounds.Value.Y;
        Width = bounds.Value.Width;
        Height = bounds.Value.Height;

        if (_settingsProvider.GetSetting(PredefinedSettings.MainWindowMaximized))
        {
            WindowState = System.Windows.WindowState.Maximized;
        }
    }

    private void SavePositionAndSize()
    {
        var windowService = (WindowService)_serviceProvider.GetService<IWindowService>()!;
        if (!windowService.IsCompactOverlayMode)
        {
            _settingsProvider.SetSetting(
                PredefinedSettings.MainWindowBounds,
                new SixLabors.ImageSharp.Rectangle(
                    (int)Left,
                    (int)Top,
                    (int)Width,
                    (int)Height));

            _settingsProvider.SetSetting(PredefinedSettings.MainWindowMaximized, WindowState == System.Windows.WindowState.Maximized);
        }
    }

    private void LogUnhandledException(Exception exception)
    {
        _logger?.LogCritical(0, exception, "Unhandled exception !!!    (╯°□°）╯︵ ┻━┻");
    }

    private void LogAppStarting()
    {
        _logger?.LogInformation(1, "App is starting...");
    }

    private void LogInitialization(double duration)
    {
        _logger?.LogInformation(2, "MEF, services and logging initialized in {duration} ms", duration);
    }

    private void LogUiLoadTime(double duration)
    {
        _logger?.LogInformation(3, "App main window's UI loaded in {duration} ms", duration);
    }

    private void LogWebViewVersion(string version)
    {
        _logger?.LogInformation(4, "Running Windows with WebView {version}", version);
    }
}
