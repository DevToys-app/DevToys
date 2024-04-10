using DevToys.Blazor.Core;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.FileProviders;

namespace DevToys.Windows.Controls.WebView;

internal sealed class CustomBlazorWebView : BlazorWebView
{
    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        var embeddedProvider = new DevToysBlazorEmbeddedFileProvider();
        var physicalProvider = new PhysicalFileProvider(contentRootDir);
        return new CompositeFileProvider(physicalProvider, embeddedProvider);
    }
}
