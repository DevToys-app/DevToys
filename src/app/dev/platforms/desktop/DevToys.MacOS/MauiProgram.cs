using DevToys.Api;
using DevToys.Blazor.Core.Languages;
using DevToys.Business.ViewModels;
using DevToys.Core.Logging;
using DevToys.Core.Mef;
using DevToys.MacOS.Core;
using DevToys.Tools;
using Microsoft.Extensions.Logging;
using PredefinedSettings = DevToys.Core.Settings.PredefinedSettings;

namespace DevToys.MacOS;

public static partial class MauiProgram
{
    private static MauiApp? app;

    private static MefComposer? _mefComposer;
    private static ILogger? _logger;

    public static MauiApp CreateMauiApp()
    {
        DateTime startTime = DateTime.Now;

        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Initialize services and logging.
        ServiceProvider serviceProvider = InitializeServices(builder.Services);

        // Listen for unhandled exceptions.
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // For iOS and Mac Catalyst
        // Exceptions will flow through AppDomain.CurrentDomain.UnhandledException,
        // but we need to set UnwindNativeCode to get it to work correctly. 
        // 
        // See: https://github.com/xamarin/xamarin-macios/issues/15252
        ObjCRuntime.Runtime.MarshalManagedException += (_, args) =>
        {
            args.ExceptionMode = ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;
        };

        // Initialize MEF.
        _mefComposer
            = new MefComposer(
                new[] {
                    typeof(DevToysToolsResourceManagerAssemblyIdentifier).Assembly,
                    typeof(MainWindowViewModel).Assembly
                });

        LogInitialization((DateTime.Now - startTime).TotalMilliseconds);
        LogAppStarting();

        MauiApp app = builder.Build();

        MauiProgram.app = app;

        // Set the user-defined language.
        string? languageIdentifier = _mefComposer.Provider.Import<ISettingsProvider>().GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l => string.Equals(l.InternalName, languageIdentifier))
            ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);

        return app;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogUnhandledException((Exception)e.ExceptionObject);
    }

    private static ServiceProvider InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddMauiBlazorWebView();

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

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MauiApp>();

        return serviceProvider;
    }

    private static void LogUnhandledException(Exception exception)
    {
        _logger?.LogCritical(0, exception, "Unhandled exception !!!    (╯°□°）╯︵ ┻━┻");
    }

    private static void LogAppStarting()
    {
        _logger?.LogInformation(1, "App is starting...");
    }

    private static void LogInitialization(double duration)
    {
        _logger?.LogInformation(2, "MEF, services and logging initialized in {duration} ms", duration);
    }
}
