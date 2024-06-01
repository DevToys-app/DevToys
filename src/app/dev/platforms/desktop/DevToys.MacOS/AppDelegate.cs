using DevToys.Api;
using DevToys.Blazor.BuiltInTools;
using DevToys.Blazor.BuiltInTools.ExtensionsManager;
using DevToys.Blazor.Core.Languages;
using DevToys.Blazor.Core.Services;
using DevToys.Business.ViewModels;
using DevToys.Core;
using DevToys.Core.Logging;
using DevToys.Core.Mef;
using DevToys.Core.Settings;
using DevToys.MacOS.Core;
using DevToys.MacOS.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ObjCRuntime;
using Constants = DevToys.MacOS.Core.Constants;

namespace DevToys.MacOS;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
    private WindowService? _windowService;
    private ILogger? _logger;

    internal static ServiceProvider? ServiceProvider;

    internal static MefComposer? MefComposer;

    public override void WillFinishLaunching(NSNotification notification)
    {
        DateTime startTime = DateTime.Now;

        // Initialize services and logging.
        ServiceProvider = InitializeServices();

        // Listen for unhandled exceptions.
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // For iOS and Mac Catalyst
        // Exceptions will flow through AppDomain.CurrentDomain.UnhandledException,
        // but we need to set UnwindNativeCode to get it to work correctly. 
        // 
        // See: https://github.com/xamarin/xamarin-macios/issues/15252
        ObjCRuntime.Runtime.MarshalManagedException += (_, args) =>
        {
            args.ExceptionMode = MarshalManagedExceptionMode.UnwindNativeCode;
            OnUnhandledException(this, new UnhandledExceptionEventArgs(args.Exception, isTerminating: true));
        };

        // Clear older temp files.
        FileHelper.ClearTempFiles(Constants.AppTempFolder);

        // Initialize extension installation folder, and uninstall extensions that are planned for being removed.
        string[] pluginFolders
            = {
                Path.Combine(NSBundle.MainBundle.ResourcePath, "Plugins"),
                Constants.PluginInstallationFolder
            };
        ExtensionInstallationManager.PreferredExtensionInstallationFolder = Constants.PluginInstallationFolder;
        ExtensionInstallationManager.ExtensionInstallationFolders = pluginFolders;
        ExtensionInstallationManager.UninstallExtensionsScheduledForRemoval();

        // Initialize MEF.
        MefComposer
            = new MefComposer(
                assemblies: new[]
                {
                    typeof(MainWindowViewModel).Assembly,
                    typeof(DevToysBlazorResourceManagerAssemblyIdentifier).Assembly
                },
                pluginFolders);

        // Initialize the window service.
        _windowService = new WindowService();

        LogInitialization((DateTime.Now - startTime).TotalMilliseconds);
        LogAppStarting();

        // Set the user-defined language.
        string languageIdentifier
            = MefComposer.Provider.Import<ISettingsProvider>().GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l =>
                  string.Equals(l.InternalName, languageIdentifier))
              ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);
    }

    public override void DidFinishLaunching(NSNotification notification)
    {
        // Create the main app menu
        NSApplication.SharedApplication.MainMenu = new AppMenuBar();

        // Show the main window
        MainWindow.Instance.ShowAsync().Forget();
    }

    public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
    {
        // The app should always shut down after every window closed.
        return true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogUnhandledException((Exception)e.ExceptionObject);
    }

    private ServiceProvider InitializeServices()
    {
        var serviceCollection = new ServiceCollection();

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

        serviceCollection.AddBlazorWebView();

        serviceCollection.AddSingleton(_ => MefComposer!.Provider);
        serviceCollection.AddSingleton<IWindowService>(_ => _windowService!);
        serviceCollection.AddScoped<DocumentEventService, DocumentEventService>();
        serviceCollection.AddScoped<PopoverService, PopoverService>();
        serviceCollection.AddScoped<ContextMenuService, ContextMenuService>();
        serviceCollection.AddScoped<GlobalDialogService, GlobalDialogService>();
        serviceCollection.AddScoped<UIDialogService, UIDialogService>();
        serviceCollection.AddScoped<FontService, FontService>();
        serviceCollection.AddScoped<MonacoLanguageService, MonacoLanguageService>();

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        LoggingExtensions.LoggerFactory = loggerFactory;
        _logger = this.Log();

        return serviceProvider;
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
}
