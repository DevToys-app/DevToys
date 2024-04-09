using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.FileProviders;

namespace DevToys.Windows.Controls.WebView;

internal sealed class CustomBlazorWebView : BlazorWebView
{
    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        System.Reflection.Assembly assembly = typeof(DevToys.Blazor.Main).Assembly;
        var embeddedProvider = new EmbeddedFileProvider(assembly);
        var physicalProvider = new PhysicalFileProvider(contentRootDir);
        return new CompositeFileProvider(physicalProvider, embeddedProvider);
    }
}
