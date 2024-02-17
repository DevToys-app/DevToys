using Microsoft.Extensions.DependencyInjection;
using WebKit;

namespace DevToys.Linux.Components;

internal sealed class BlazorWebView : WebView
{
    private readonly BlazorWebViewBridge _bridge;

    static BlazorWebView()
    {
        WebKit.Module.Initialize();
    }

    internal BlazorWebView(IServiceProvider serviceProvider)
    {
        BlazorWebViewOptions options = serviceProvider.GetRequiredService<BlazorWebViewOptions>();

        // We assume the host page is always in the root of the content directory, because it's
        // unclear there's any other use case. We can add more options later if so.
        string contentRootDir = Path.GetDirectoryName(options.HostPath) ?? string.Empty;
        string hostPageRelativePath = Path.GetRelativePath(contentRootDir, options.HostPath);

        _bridge
            = new BlazorWebViewBridge(
                this,
                serviceProvider,
                options,
                contentRootDir,
                hostPageRelativePath,
                OnBlazorInitialized);
    }

    internal event EventHandler? BlazorWebViewInitialized;

    private void OnBlazorInitialized()
    {
        BlazorWebViewInitialized?.Invoke(this, EventArgs.Empty);
    }
}
