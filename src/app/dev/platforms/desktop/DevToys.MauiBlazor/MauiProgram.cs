using Microsoft.Extensions.Logging;
using Microsoft.Fast.Components.FluentUI.Infrastructure;
using Microsoft.Fast.Components.FluentUI;
using DevToys.Core.Mef;

namespace DevToys.MauiBlazor;

public static class MauiProgram
{
    private static readonly MefComposer mefComposer = new();

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

        builder.Services.AddSingleton(provider => mefComposer.Provider);

        return builder.Build();
    }
}
