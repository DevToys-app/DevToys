﻿using DevToys.Api;
using DevToys.Blazor.BuiltInTools;
using DevToys.Blazor.BuiltInTools.ExtensionsManager;
using DevToys.Blazor.Core.Languages;
using DevToys.Blazor.Core.Services;
using DevToys.Business.ViewModels;
using DevToys.Core.Logging;
using DevToys.Core.Mef;
using DevToys.MacOS.Core;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PredefinedSettings = DevToys.Core.Settings.PredefinedSettings;
using DevToys.Core;

namespace DevToys.MacOS;

public partial class MauiProgram
{
    private readonly WindowService _windowService = new();
    private ILogger? _logger;
    private MauiApp? _app;

    internal static ServiceProvider? ServiceProvider;

    internal static MefComposer? MefComposer;

    public MauiApp CreateMauiApp()
    {
        Guard.IsNull(_app);

        DateTime startTime = DateTime.Now;

        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Initialize the .NET MAUI Community Toolkit by adding the below line of code
        builder.UseMauiCommunityToolkit();

        // Initialize services and logging.
        ServiceProvider = InitializeServices(builder.Services);

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

        // Clear older temp files.
        FileHelper.ClearTempFiles(Constants.AppTempFolder);

        // Initialize extension installation folder, and uninstall extensions that are planned for being removed.
        string[] pluginFolders
            = new[]
            {
                Path.Combine(AppContext.BaseDirectory!, "../Resources/Plugins"),
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

        LogInitialization((DateTime.Now - startTime).TotalMilliseconds);
        LogAppStarting();

        _app = builder.Build();

        // Set the user-defined language.
        string? languageIdentifier = MefComposer.Provider.Import<ISettingsProvider>().GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l => string.Equals(l.InternalName, languageIdentifier))
            ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);

        return _app;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogUnhandledException((Exception)e.ExceptionObject);
    }

    private ServiceProvider InitializeServices(IServiceCollection serviceCollection)
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

        serviceCollection.AddSingleton(provider => MefComposer!.Provider);
        serviceCollection.AddSingleton<IWindowService>(provider => _windowService);
        serviceCollection.AddScoped<DocumentEventService, DocumentEventService>();
        serviceCollection.AddScoped<PopoverService, PopoverService>();
        serviceCollection.AddScoped<ContextMenuService, ContextMenuService>();
        serviceCollection.AddScoped<DialogService, DialogService>();
        serviceCollection.AddScoped<FontService, FontService>();

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
