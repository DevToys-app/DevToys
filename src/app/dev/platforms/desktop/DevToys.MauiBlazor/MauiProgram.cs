using DevToys.Api;
using DevToys.Business.ViewModels;
using DevToys.Core.Logging;
using DevToys.Core.Mef;
using DevToys.MauiBlazor.Core.FileStorage;
using DevToys.MauiBlazor.Core.Languages;
using DevToys.Tools;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
using PredefinedSettings = DevToys.Core.Settings.PredefinedSettings;

namespace DevToys.MauiBlazor;

public static partial class MauiProgram
{
    internal static readonly Lazy<MefComposer> MefComposer
        = new(() => new MefComposer(
            new[] {
                typeof(DevToysToolsResourceManagerAssemblyIdentifier).Assembly,
                typeof(MainWindowViewModel).Assembly
            }));

    private static MauiApp? app;

    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("System", LogLevel.Warning);
        builder.Logging.AddFile(new FileStorage());

        builder.Services.AddSingleton(provider => MefComposer.Value.Provider);

        MauiApp app = builder.Build();

        MauiProgram.app = app;
        GlobalExceptionHandler.UnhandledException += GlobalExceptionHandler_UnhandledException;

        ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = loggerFactory;

        app.Log().LogInformation("App is starting...");

        // Set the user-defined language.
        string? languageIdentifier = MefComposer.Value.Provider.Import<ISettingsProvider>().GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l => string.Equals(l.InternalName, languageIdentifier))
            ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);

        return app;
    }

    private static void GlobalExceptionHandler_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Guard.IsNotNull(app);
        LogUnhandledException(app.Log(), (Exception)e.ExceptionObject);
    }

    [LoggerMessage(5, LogLevel.Critical, "Unhandled exception !!!    (╯°□°）╯︵ ┻━┻")]
    static partial void LogUnhandledException(ILogger logger, Exception exception);
}
