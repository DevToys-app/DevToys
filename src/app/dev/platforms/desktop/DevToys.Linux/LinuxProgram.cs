using DevToys.Api;
using DevToys.Blazor.BuiltInTools;
using DevToys.Blazor.BuiltInTools.ExtensionsManager;
using DevToys.Blazor.Core.Languages;
using DevToys.Blazor.Core.Services;
using DevToys.Business.ViewModels;
using DevToys.Core.Mef;
using DevToys.Core.Settings;
using DevToys.Linux.Components;
using DevToys.Linux.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevToys.Core.Logging;

namespace DevToys.Linux;

internal partial class LinuxProgram
{
    internal static ILogger? Logger;
    private static MefComposer? MefComposer;

    private readonly WindowService _windowService = new();
    private readonly ServiceCollection _serviceCollection = new();
    private readonly DateTime _startTime = DateTime.Now;

    private MainWindow? _mainWindow;

    internal LinuxProgram()
    {
        WebKit.Module.Initialize();

        Application = Adw.Application.New("devtoys", Gio.ApplicationFlags.FlagsNone);

        Application.OnActivate += OnApplicationActivate;
    }

    internal Adw.Application Application { get; }

    private void OnApplicationActivate(object sender, object e)
    {
        // Initialize services and logging.
        ServiceProvider serviceProvider = InitializeServices();

        // Listen for unhandled exceptions.
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
        {
            LogUnhandledException((Exception)ex.ExceptionObject);
        };

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
        MefComposer
            = new MefComposer(
                assemblies: new[] {
                        typeof(MainWindowViewModel).Assembly,
                        typeof(DevToysBlazorResourceManagerAssemblyIdentifier).Assembly
                },
                pluginFolders);

        LogInitialization((DateTime.Now - _startTime).TotalMilliseconds);
        LogAppStarting();

        // Set the user-defined language.
        string? languageIdentifier = MefComposer.Provider.Import<ISettingsProvider>().GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l => string.Equals(l.InternalName, languageIdentifier))
            ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);

        // Create and open main window.
        _mainWindow = new MainWindow(serviceProvider, (Adw.Application)sender);
    }

    private ServiceProvider InitializeServices()
    {
        _serviceCollection.AddBlazorWebView();

        _serviceCollection.AddSingleton(
            new BlazorWebViewOptions()
            {
                RootComponent = typeof(DevToys.Blazor.Main)
            }
        );

        _serviceCollection.AddLogging((ILoggingBuilder builder) =>
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

        _serviceCollection.AddSingleton(provider => MefComposer!.Provider);
        _serviceCollection.AddSingleton<IWindowService>(provider => _windowService);
        _serviceCollection.AddScoped<DocumentEventService, DocumentEventService>();
        _serviceCollection.AddScoped<PopoverService, PopoverService>();
        _serviceCollection.AddScoped<ContextMenuService, ContextMenuService>();
        _serviceCollection.AddScoped<DialogService, DialogService>();
        _serviceCollection.AddScoped<FontService, FontService>();

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        LoggingExtensions.LoggerFactory = loggerFactory;
        Logger = this.Log();

        return serviceProvider;
    }

    private static void LogUnhandledException(Exception exception)
    {
        Logger?.LogCritical(0, exception, "Unhandled exception !!!    (╯°□°）╯︵ ┻━┻");
    }

    private static void LogAppStarting()
    {
        Logger?.LogInformation(1, "App is starting...");
    }

    private static void LogInitialization(double duration)
    {
        Logger?.LogInformation(2, "MEF, services and logging initialized in {duration} ms", duration);
    }
}
