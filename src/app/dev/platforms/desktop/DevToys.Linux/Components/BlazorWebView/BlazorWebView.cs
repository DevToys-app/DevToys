using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;
using DevToys.Api;
using DevToys.Blazor.Core;
using Gdk;
using Gio.Internal;
using JavaScriptCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Soup;
using WebKit;
using Exception = System.Exception;
using Module = WebKit.Module;
using Settings = WebKit.Settings;
using Task = System.Threading.Tasks.Task;
using Uri = System.Uri;

namespace DevToys.Linux.Components;

internal sealed partial class BlazorWebView : IDisposable
{
    private const string DevToysInteropName = "devtoyswebinterop";
    private const string Scheme = "app";
    internal const string AppHostAddress = "localhost";
    internal static readonly Uri AppOriginUri = new($"{Scheme}://{AppHostAddress}/");

    private const string BlazorInitScript
        = $$"""
            window.__receiveMessageCallbacks = [];
            window.__dispatchMessageCallback = function(message) {
                window.__receiveMessageCallbacks.forEach(
                    function(callback)
                    {
                        try
                        {
                            callback(message);
                        }
                        catch { }
                    });
            };
            window.external = {
                sendMessage: function(message) {
                    window.webkit.messageHandlers.{{DevToysInteropName}}.postMessage(message);
                },
                receiveMessage: function(callback) {
                    window.__receiveMessageCallbacks.push(callback);
                }
            };

            try
            {
                Blazor.start(); // It might have already started.
            }
            catch {}

            (function () {
                window.onpageshow = function(event) {
                    if (event.persisted) {
                        window.location.reload();
                    }
                };
            })();
            """;

    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSchemeHandler _appSchemeHandler;
    private readonly bool _enabledDeveloperTools;

    private BlazorWebViewManager? _webViewManager;
    private string? _hostPage;

    static BlazorWebView()
    {
        Module.Initialize();
    }

    internal BlazorWebView(IServiceProvider serviceProvider, bool enableDeveloperTools)
    {
        _logger = this.Log();
        Guard.IsNotNull(serviceProvider);
        _serviceProvider = serviceProvider;
        _appSchemeHandler = new AppSchemeHandler(this);

        _enabledDeveloperTools = enableDeveloperTools;
        View = CreateWebView();

        View.OnContextMenu += BlazorGtkWebViewOnContextMenu;
        RootComponents.CollectionChanged += RootComponentsOnCollectionChanged;
    }

    /// <summary>
    /// Gets the view corresponding to the current Blazor web view.
    /// </summary>
    internal WebView View { get; }

    /// <summary>
    /// Path to the host page within the application's static files. For example, <code>wwwroot\index.html</code>.
    /// This property must be set to a valid value for the Razor components to start.
    /// </summary>
    internal string? HostPage
    {
        get => _hostPage;
        set
        {
            _hostPage = value;
            StartWebViewCoreIfPossible();
        }
    }

    /// <summary>
    /// Gets or sets the path for initial navigation within the Blazor navigation context when the Blazor component is finished loading.
    /// </summary>
    internal string StartPath { get; set; } = "/";

    /// <summary>
    /// A collection of <see cref="RootComponent"/> instances that specify the Blazor <see cref="IComponent"/> types
    /// to be used directly in the specified <see cref="HostPage"/>.
    /// </summary>
    internal RootComponentsCollection RootComponents { get; } = new();

    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    internal event EventHandler<UrlLoadingEventArgs>? UrlLoading;

    internal event EventHandler? BlazorWebViewInitialized;

    public void Dispose()
    {
        if (_webViewManager is not null)
        {
            // Dispose this component's contents and block on completion so that user-written disposal logic and
            // Blazor disposal logic will complete.
            _webViewManager?
                .DisposeAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();

            _webViewManager = null;
        }

        View.Dispose();
    }

    internal void OnUrlLoading(UrlLoadingEventArgs args)
    {
        Guard.IsNotNull(args);
        UrlLoading?.Invoke(this, args);
    }

    private bool BlazorGtkWebViewOnContextMenu(WebView sender, WebView.ContextMenuSignalArgs args)
    {
        // Returning true to prevent the context menu from opening.
        return !_enabledDeveloperTools;
    }

    private void RootComponentsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // If we haven't initialized yet, this is a no-op
        if (_webViewManager is not null)
        {
            // Dispatch because this is going to be async, and we want to catch any errors
            _webViewManager.Dispatcher.InvokeAsync(async () =>
            {
                IEnumerable<RootComponent> newItems = e.NewItems!.Cast<RootComponent>().ToArray();
                IEnumerable<RootComponent> oldItems = e.OldItems!.Cast<RootComponent>().ToArray();

                foreach (RootComponent? item in newItems.Except(oldItems))
                {
                    await item.AddToWebViewManagerAsync(_webViewManager);
                }

                foreach (RootComponent? item in oldItems.Except(newItems))
                {
                    await item.RemoveFromWebViewManagerAsync(_webViewManager);
                }
            }).Forget();
        }
    }

    private void MessageReceived(Uri uri, string message)
    {
        _webViewManager?.MessageReceivedInternal(uri, message);
    }

    private WebView CreateWebView()
    {
        var webView = new WebView();

        try
        {
            Guard.IsNotNull(webView
                .WebContext); // this might be null or crash when trying to access the WebContext property.
        }
        catch (Exception e)
        {
            throw new DllNotFoundException(
                "WebKitGTK could not be found. Please verify that the package 'libwebkitgtk-6.0-4' is installed on the operating system and retry.");
        }

        // Make web view transparent
        webView.SetBackgroundColor(new RGBA { Red = 255, Blue = 0, Green = 0, Alpha = 0 });

        // Initialize some basic properties of the WebView
        Settings webViewSettings = webView.GetSettings();
        webViewSettings.EnableDeveloperExtras = _enabledDeveloperTools;
        webViewSettings.JavascriptCanAccessClipboard = true;
        webViewSettings.EnableBackForwardNavigationGestures = false;
        webViewSettings.MediaPlaybackRequiresUserGesture = false;
        webViewSettings.HardwareAccelerationPolicy = HardwareAccelerationPolicy.Never; // https://github.com/DevToys-app/DevToys/issues/1234
        webView.SetSettings(webViewSettings);

        UserContentManager userContentManager = webView.GetUserContentManager();

        // Handle messages.
        UserContentManager.ScriptMessageReceivedSignal.Connect(
            userContentManager,
            HandleScriptMessageReceivedSignal,
            after: false,
            detail: DevToysInteropName);
        if (!userContentManager.RegisterScriptMessageHandler(DevToysInteropName, null))
        {
            throw new Exception("Could not register script message handler");
        }

        // Add Blazor initialization script.
        userContentManager.AddScript(
            UserScript.New(
                BlazorInitScript,
                injectedFrames: UserContentInjectedFrames.AllFrames,
                injectionTime: UserScriptInjectionTime.End,
                allowList: null,
                blockList: null));

        // Register a "app" url scheme to handle Blazor resources
        webView.WebContext.RegisterUriScheme(Scheme, HandleUriScheme);

        return webView;
    }

    private void HandleScriptMessageReceivedSignal(
        UserContentManager ucm,
        UserContentManager.ScriptMessageReceivedSignalArgs signalArgs)
    {
        Value result = signalArgs.Value;
        MessageReceived(AppOriginUri, result.ToString());
    }

    private void HandleUriScheme(URISchemeRequest request)
    {
        _appSchemeHandler.StartUrlSchemeTask(request);
    }

    private void StartWebViewCoreIfPossible()
    {
        if (HostPage == null || _webViewManager != null)
        {
            return;
        }

        // We assume the host page is always in the root of the content directory, because it's
        // unclear there's any other use case. We can add more options later if so.
        string contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
        string hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

        LogCreatingFileProvider(contentRootDir, hostPageRelativePath);

        IFileProvider fileProvider = CreateFileProvider(contentRootDir);

        _webViewManager = new BlazorWebViewManager(
            AppOriginUri,
            this,
            _serviceProvider,
            fileProvider,
            RootComponents.JSComponents,
            contentRootDir,
            hostPageRelativePath);

        StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webViewManager);

        foreach (RootComponent rootComponent in RootComponents)
        {
            LogAddingRootComponent(rootComponent.ComponentType?.FullName ?? string.Empty,
                rootComponent.Selector ?? string.Empty, rootComponent.Parameters?.Count ?? 0);

            // Since the page isn't loaded yet, this will always complete synchronously
            _ = rootComponent.AddToWebViewManagerAsync(_webViewManager);
        }

        LogStartingInitialNavigation(StartPath);
        _webViewManager.Navigate(StartPath);

        Task.Run(async () =>
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);
            BlazorWebViewInitialized?.Invoke(this, EventArgs.Empty);
        });
    }

    private static IFileProvider CreateFileProvider(string contentRootDir)
    {
        string contentRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        string bundleRootDir = Path.Combine(contentRoot, contentRootDir);
        var physicalProvider = new PhysicalFileProvider(bundleRootDir);
        var embeddedProvider = new DevToysBlazorEmbeddedFileProvider();
        return new CompositeFileProvider(physicalProvider, embeddedProvider);
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug,
        Message =
            "Creating file provider at content root '{contentRootDir}', using host page relative path '{hostPageRelativePath}'.")]
    partial void LogCreatingFileProvider(string contentRootDir, string hostPageRelativePath);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug,
        Message =
            "Adding root component '{componentTypeName}' with selector '{componentSelector}'. Number of parameters: {parameterCount}")]
    partial void LogAddingRootComponent(string componentTypeName, string componentSelector, int parameterCount);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Starting initial navigation to '{startPath}'.")]
    partial void LogStartingInitialNavigation(string startPath);

    private sealed class AppSchemeHandler
    {
        private readonly BlazorWebView _blazorWebView;

        internal AppSchemeHandler(BlazorWebView blazorWebView)
        {
            _blazorWebView = blazorWebView;
        }

        public void StartUrlSchemeTask(URISchemeRequest urlSchemeTask)
        {
            if (urlSchemeTask.GetScheme() != Scheme)
            {
                throw new Exception($"Invalid scheme '{urlSchemeTask.GetScheme()}'");
            }

            string uri = urlSchemeTask.GetUri();

            byte[] responseBytes
                = GetResponseBytes(
                    uri,
                    out string contentType,
                    out int statusCode,
                    out string statusMessage);

            if (statusCode != 200)
            {
                return;
            }

            // g_memory_input_stream_new_from_data() does NOT copy the buffer: it keeps the raw
            // pointer and reads from it asynchronously (after this method returns), calling the
            // destroy notify only once WebKitGTK is done with the stream. The previous code handed
            // it a pointer into a managed, unpinned MemoryStream buffer together with a no-op
            // destroy notify, so the GC was free to move, collect or reuse that memory before/while
            // WebKitGTK read it. The result was corrupted responses (zeroed bytes, and bytes from
            // other concurrent requests bleeding in), which broke script parsing in JavaScriptCore
            // ("SyntaxError: Invalid character: '\\0'", garbled assets) and left the `devtoys` global
            // undefined. Pin the response buffer for the whole lifetime of the stream and release it
            // from the destroy notify.
            // A non-empty backing array is required so `ref buffer[0]` is valid even for an empty
            // body; the exposed length stays responseBytes.Length.
            byte[] buffer = responseBytes.Length == 0 ? new byte[1] : responseBytes;
            GCHandle responseHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            nint streamPtr = MemoryInputStream.NewFromData(
                ref buffer[0], (uint)responseBytes.Length, _ => responseHandle.Free());
            using var inputStream = new InputStream(streamPtr, false);

            var headers = MessageHeaders.New(MessageHeadersType.Response);
            headers.SetContentLength(responseBytes.Length);

            // Disable local caching. This will prevent user scripts from executing correctly.
            headers.Append("Cache-Control", "no-cache, max-age=0, must-revalidate, no-store");

            var response = URISchemeResponse.New(inputStream, responseBytes.Length);
            response.SetHttpHeaders(headers);
            response.SetContentType(contentType);
            response.SetStatus((uint)statusCode, statusMessage);

            urlSchemeTask.FinishWithResponse(response);
        }

        private byte[] GetResponseBytes(string? url, out string contentType, out int statusCode,
            out string statusMessage)
        {
            bool allowFallbackOnHostPage = IsUriBaseOfPage(AppOriginUri, url);
            url = RemovePossibleQueryString(url);

            if (_blazorWebView._webViewManager!.TryGetResponseContentInternal(
                    url,
                    allowFallbackOnHostPage,
                    out _,
                    out statusMessage,
                    out Stream content,
                    out IDictionary<string, string> headers))
            {
                statusCode = 200;
                using var ms = new MemoryStream();

                content.CopyTo(ms);
                content.Dispose();

                contentType = headers["Content-Type"];

                return ms.ToArray();
            }

            statusCode = 404;
            contentType = string.Empty;
            return Array.Empty<byte>();
        }

        private static string RemovePossibleQueryString(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            int indexOfQueryString = url.IndexOf('?', StringComparison.Ordinal);
            return indexOfQueryString == -1
                ? url
                : url.Substring(0, indexOfQueryString);
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
