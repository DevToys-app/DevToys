using System.Web;
using DevToys.Api;
using Gio.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebKit;

namespace DevToys.Linux.Components;

internal sealed class BlazorWebViewBridge : WebViewManager
{
    private const string Scheme = "app";
    private static readonly Uri baseUri = new($"{Scheme}://localhost/");

    private readonly BlazorWebView _webView;
    private readonly string _relativeHostPath;
    private readonly ILogger _logger;

    public BlazorWebViewBridge(
        BlazorWebView webView,
        IServiceProvider serviceProvider)
        : base(
            serviceProvider,
            Dispatcher.CreateDefault(),
            baseUri,
            new PhysicalFileProvider(serviceProvider.GetRequiredService<BlazorWebViewOptions>().ContentRoot),
            new JSComponentConfigurationStore(),
            serviceProvider.GetRequiredService<BlazorWebViewOptions>().RelativeHostPath)
    {
        Guard.IsNotNull(webView);
        _webView = webView;
        _logger = this.Log();

        BlazorWebViewOptions options = serviceProvider.GetRequiredService<BlazorWebViewOptions>();
        _relativeHostPath = options.RelativeHostPath;
        Type rootComponent = options.RootComponent;

        // This is necessary to automatically serve the files in the `_framework` virtual folder.
        // Using `file://` will cause the webview to look for the `_framework` files on the file system,
        // and it won't find them.
        if (_webView.WebContext is null)
        {
            throw new Exception("WebView.WebContext is null");
        }

        _webView.WebContext.RegisterUriScheme(Scheme, HandleUriScheme);

        Dispatcher.InvokeAsync(async () =>
        {
            await AddRootComponentAsync(rootComponent, "#app", ParameterView.Empty);
        });

        UserContentManager ucm = webView.GetUserContentManager();
        ucm.AddScript(UserScript.New(
            source:
            """
                window.__receiveMessageCallbacks = [];

                window.__dispatchMessageCallback = function(message) {
                    window.__receiveMessageCallbacks.forEach(function(callback) { callback(message); });
                };

                window.external = {
                    sendMessage: function(message) {
                        window.webkit.messageHandlers.webview.postMessage(message);
                    },
                    receiveMessage: function(callback) {
                        window.__receiveMessageCallbacks.push(callback);
                    }
                };
            """,
            injectedFrames: UserContentInjectedFrames.AllFrames,
            injectionTime: UserScriptInjectionTime.Start)
        );

        UserContentManager.ScriptMessageReceivedSignal.Connect(
            ucm,
            (_, signalArgs) =>
            {
                JavaScriptCore.Value result = signalArgs.Value;
                MessageReceived(baseUri, result.ToString());
            },
            true,
            "webview");

        if (!ucm.RegisterScriptMessageHandler("webview", null))
        {
            throw new Exception("Could not register script message handler");
        }

        Navigate("/");
    }

    protected override void NavigateCore(Uri absoluteUri)
    {
        _logger?.LogDebug($"Navigating to \"{absoluteUri}\"");
        _webView.LoadUri(absoluteUri.ToString());
    }

    protected override async void SendMessage(string message)
    {
        string script = $"__dispatchMessageCallback(\"{HttpUtility.JavaScriptStringEncode(message)}\")";
        _logger?.LogDebug($"Dispatching `{script}`");
        _ = await _webView.EvaluateJavascriptAsync(script);
    }

    private void HandleUriScheme(URISchemeRequest request)
    {
        if (request.GetScheme() != Scheme)
        {
            throw new Exception($"Invalid scheme '{request.GetScheme()}'");
        }

        string uri = request.GetUri();
        if (request.GetPath() == "/")
        {
            uri += _relativeHostPath;
        }

        _logger?.LogDebug($"Fetching '{uri}'");

        if (TryGetResponseContent(
            uri,
            false,
            out int statusCode,
            out string? statusMessage,
            out Stream? content,
            out IDictionary<string, string>? headers))
        {
            using var ms = new MemoryStream();
            content.CopyTo(ms);
            nint streamPtr = MemoryInputStream.NewFromData(ref ms.GetBuffer()[0], (uint)ms.Length, _ => { });
            var inputStream = new InputStream(streamPtr, false);
            request.Finish(inputStream, ms.Length, headers["Content-Type"]);
            // content.Dispose(); // TODO: Is this necessary?
        }
        else
        {
            throw new Exception($"Failed to serve \"{uri}\". {statusCode} - {statusMessage}");
        }
    }

    // Workaround for protection level access
    private class InputStream : Gio.InputStream
    {
        protected internal InputStream(IntPtr ptr, bool ownedRef)
            : base(ptr, ownedRef)
        {
        }
    }
}
