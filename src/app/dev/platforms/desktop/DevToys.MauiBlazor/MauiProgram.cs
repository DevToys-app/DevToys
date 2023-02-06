using DevToys.Api;
using DevToys.Core.Mef;
using DevToys.Core.Settings;
using DevToys.MauiBlazor.Core.Languages;
using Microsoft.Extensions.Logging;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.Infrastructure;

namespace DevToys.MauiBlazor;

public static class MauiProgram
{
    internal static readonly MefComposer MefComposer = new();

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

        builder.Services.AddSingleton(provider => MefComposer.Provider);

        // Set the user-defined language.
        string? languageIdentifier = MefComposer.Provider.Import<ISettingsProvider>().GetSetting(PredefinedSettings.Language);
        LanguageDefinition languageDefinition
            = LanguageManager.Instance.AvailableLanguages.FirstOrDefault(l => string.Equals(l.InternalName, languageIdentifier))
            ?? LanguageManager.Instance.AvailableLanguages[0];
        LanguageManager.Instance.SetCurrentCulture(languageDefinition);

        return builder.Build();
    }
}
