using System.Web;
using DevToys.Api;
using Gio.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using WebKit;
using Action = System.Action;

namespace DevToys.Linux.Components;

internal sealed class BlazorWebViewBridge : WebViewManager
{
    private const string Scheme = "app";
    private static readonly Uri AppOriginUri = new($"{Scheme}://localhost/");

    private readonly BlazorWebView _webView;
    private readonly string _relativeHostPath;

    public BlazorWebViewBridge(
        BlazorWebView webView,
        IServiceProvider serviceProvider,
        BlazorWebViewOptions options,
        IFileProvider fileProvider,
        string hostPageRelativePath,
        Action onBlazorInitialized)
        : base(
            serviceProvider,
            Dispatcher.CreateDefault(),
            AppOriginUri,
            fileProvider,
            new JSComponentConfigurationStore(),
            hostPageRelativePath)
    {
        Guard.IsNotNull(webView);
        _webView = webView;

        _relativeHostPath = options.RelativeHostPath;
        Type rootComponent = options.RootComponent;

        try
        {
            Guard.IsNotNull(_webView.WebContext); // this might be null or crash when trying to access the WebContext property.
        }
        catch (Exception e)
        {
            throw new DllNotFoundException("WebKitGTK could not be found. Please verify that the package 'libwebkitgtk-6.0-4' is installed on the operating system and retry.");
        }

        // This is necessary to automatically serve the files in the `_framework` virtual folder.
        // Using `file://` will cause the webview to look for the `_framework` files on the file system,
        // and it won't find them.
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
            HandleScriptMessageReceivedSignal,
            true,
            "webview");

        if (!ucm.RegisterScriptMessageHandler("webview", null))
        {
            throw new Exception("Could not register script message handler");
        }

        Navigate("/");

        System.Threading.Tasks.Task.Run(async () =>
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);
            onBlazorInitialized.Invoke();
        });
    }

    protected override void NavigateCore(Uri absoluteUri)
    {
        _webView.LoadUri(absoluteUri.ToString());
    }

    protected override void SendMessage(string message)
    {
        string script = $"__dispatchMessageCallback(\"{HttpUtility.JavaScriptStringEncode(message)}\")";
        _webView.EvaluateJavascriptAsync(script).Forget();
    }

    private void HandleScriptMessageReceivedSignal(UserContentManager ucm, UserContentManager.ScriptMessageReceivedSignalArgs signalArgs)
    {
        JavaScriptCore.Value result = signalArgs.Value;
        MessageReceived(AppOriginUri, result.ToString());
    }

    private void HandleUriScheme(URISchemeRequest request)
    {
        if (request.GetScheme() != Scheme)
        {
            throw new Exception($"Invalid scheme '{request.GetScheme()}'");
        }

        string uri = request.GetUri();
        bool allowFallbackOnHostPage = IsUriBaseOfPage(AppOriginUri, uri);

        if (TryGetResponseContent(
                uri,
                allowFallbackOnHostPage,
                out int statusCode,
                out string? statusMessage,
                out Stream? content,
                out IDictionary<string, string>? headers))
        {
            using var ms = new MemoryStream();
            content.CopyTo(ms);
            content.Dispose();
            nint streamPtr = MemoryInputStream.NewFromData(ref ms.GetBuffer()[0], (uint)ms.Length, _ => { });
            using var inputStream = new InputStream(streamPtr, false);
            request.Finish(inputStream, ms.Length, headers["Content-Type"]);
        }
        else
        {
            throw new Exception($"Failed to serve \"{uri}\". {statusCode} - {statusMessage}");
        }
    }

    private static bool IsUriBaseOfPage(Uri baseUri, string? uriString)
    {
        if (Path.HasExtension(uriString))
        {
            // If the path ends in a file extension, it's not referring to a page.
            return false;
        }

        var uri = new Uri(uriString!);
        return baseUri.IsBaseOf(uri);
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
