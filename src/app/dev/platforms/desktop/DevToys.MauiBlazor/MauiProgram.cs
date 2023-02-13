using DevToys.Api;
using DevToys.Core.Logging;
using DevToys.Core.Mef;
using DevToys.Core.Settings;
using DevToys.MauiBlazor.Core.FileStorage;
using DevToys.MauiBlazor.Core.Languages;
using Microsoft.Extensions.Logging;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.Infrastructure;
using Uno.Extensions;

namespace DevToys.MauiBlazor;

public static class MauiProgram
{
    internal static readonly Lazy<MefComposer> MefComposer = new(() => new MefComposer());

    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();

        // Initialize Microsoft.Fast.Components.FluentUI
        builder.Services.AddFluentUIComponents(options =>
        {
            Guard.IsNotNull(options);
            options.HostingModel = BlazorHostingModel.Hybrid;
        });
        builder.Services.AddScoped<IStaticAssetService, FileBasedStaticAssetService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("System", LogLevel.Warning);
        builder.Logging.AddFile(new FileStorage());

        builder.Services.AddSingleton(provider => MefComposer.Value.Provider);

        MauiApp app = builder.Build();

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
}
