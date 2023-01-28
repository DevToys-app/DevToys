using Microsoft.Extensions.Logging;
using Microsoft.Fast.Components.FluentUI.Infrastructure;
using Microsoft.Fast.Components.FluentUI;

namespace DevToys.MauiBlazor;

public static class MauiProgram
{
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

        return builder.Build();
    }
}
