using System.Collections.Specialized;
using System.Globalization;
using DevToys.Api;
using DevToys.Blazor.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebKit;

namespace DevToys.MacOS.Controls.BlazorWebView;

internal sealed partial class BlazorWkWebView : IDisposable
{
    private const string DevToysInteropName = "devtoyswebinterop";
    private const string Scheme = "app";

    // On MacOS 15.0, "0.0.0.0" doesn't work somehow. Instead, let's use localhost.
    // See https://github.com/DevToys-app/DevToys/issues/1279 and https://github.com/dotnet/maui/issues/23390
    internal static readonly Lazy<string> AppHostAddress = new(() => OperatingSystem.IsMacOSVersionAtLeast(15, 0) ? "localhost" : "0.0.0.0");
    internal static readonly Lazy<Uri> AppOriginUri = new(() => new Uri($"{Scheme}://{AppHostAddress.Value}/"));

    private const string BlazorInitScript
        = $$"""
            window.__receiveMessageCallbacks = [];
            window.__dispatchMessageCallback = function(message) {
                window.__receiveMessageCallbacks.forEach(function(callback) { callback(message); });
            };
            window.external = {
                sendMessage: function(message) {
                    window.webkit.messageHandlers.{{DevToysInteropName}}.postMessage(message);
                },
                receiveMessage: function(callback) {
                    window.__receiveMessageCallbacks.push(callback);
                }
            };
            
            Blazor.start();
            
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

    private BlazorWebViewManager? _webViewManager;
    private string? _hostPage;

    internal BlazorWkWebView(IServiceProvider serviceProvider, bool enableDeveloperTools)
    {
        _logger = this.Log();
        Guard.IsNotNull(serviceProvider);
        _serviceProvider = serviceProvider;
        View = CreateWebView(enableDeveloperTools);

        RootComponents.CollectionChanged += RootComponentsOnCollectionChanged;
    }

    /// <summary>
    /// Gets the view corresponding to the current Blazor web view.
    /// </summary>
    internal WKWebView View { get; }

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

    private WKWebView CreateWebView(bool enableDeveloperTools)
    {
        WKWebViewConfiguration configuration = CreateConfiguration(enableDeveloperTools, MessageReceived);

        var webView = new WKWebView(CGRect.Empty, configuration)
        {
            // Initialize some basic properties of the WebView
            AllowsBackForwardNavigationGestures = false,
            AllowsLinkPreview = false,
            AllowsMagnification = true,
            AutoresizesSubviews = true
        };

        // Make the WKWebView transparent
        webView.SetValueForKey(NSObject.FromObject(false), new NSString("drawsBackground"));

        if (OperatingSystem.IsIOSVersionAtLeast(16, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 3))
        {
            // Enable Developer Extras for Catalyst/iOS builds for 16.4+
            webView.SetValueForKey(NSObject.FromObject(enableDeveloperTools), new NSString("inspectable"));
        }

        return webView;
    }

    private WKWebViewConfiguration CreateConfiguration(bool enableDeveloperTools, Action<Uri, string> messageReceivedDelegate)
    {
        var config = new WKWebViewConfiguration();

        // By default, setting inline media playback to forbidden, including autoplay
        // and picture in picture, since these things MUST be set during the webview
        // creation, and have no effect if set afterwards.
        // A custom handler factory delegate could be set to disable these defaults
        // but if we do not set them here, they cannot be changed once the
        // handler's platform view is created, so erring on the side of wanting this
        // capability by default.
        if (OperatingSystem.IsMacCatalystVersionAtLeast(10) || OperatingSystem.IsIOSVersionAtLeast(10))
        {
            config.AllowsAirPlayForMediaPlayback = false;
            config.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
        }

        // Handle messages.
        config.UserContentController.AddScriptMessageHandler(
            new WebViewScriptMessageHandler(messageReceivedDelegate),
            DevToysInteropName);

        // Add Blazor initialization script.
        config.UserContentController.AddUserScript(
            new WKUserScript(
                new NSString(BlazorInitScript),
                WKUserScriptInjectionTime.AtDocumentEnd,
                isForMainFrameOnly: true));

        // Register a "app" url scheme to handle Blazor resources
        config.SetUrlSchemeHandler(new AppSchemeHandler(this), urlScheme: Scheme);

        // Legacy Developer Extras setting.
        config.Preferences.SetValueForKey(
            NSObject.FromObject(enableDeveloperTools),
            new NSString("developerExtrasEnabled"));

        return config;
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
            AppOriginUri.Value,
            this,
            _serviceProvider,
            fileProvider,
            RootComponents.JSComponents,
            contentRootDir,
            hostPageRelativePath);

        StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webViewManager);

        foreach (RootComponent rootComponent in RootComponents)
        {
            LogAddingRootComponent(rootComponent.ComponentType?.FullName ?? string.Empty, rootComponent.Selector ?? string.Empty, rootComponent.Parameters?.Count ?? 0);

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
        string bundleRootDir = Path.Combine(NSBundle.MainBundle.ResourcePath, contentRootDir);
        var physicalProvider = new PhysicalFileProvider(bundleRootDir);
        var embeddedProvider = new DevToysBlazorEmbeddedFileProvider();
        return new CompositeFileProvider(physicalProvider, embeddedProvider);
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Creating file provider at content root '{contentRootDir}', using host page relative path '{hostPageRelativePath}'.")]
    partial void LogCreatingFileProvider(string contentRootDir, string hostPageRelativePath);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Adding root component '{componentTypeName}' with selector '{componentSelector}'. Number of parameters: {parameterCount}")]
    partial void LogAddingRootComponent(string componentTypeName, string componentSelector, int parameterCount);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Starting initial navigation to '{startPath}'.")]
    partial void LogStartingInitialNavigation(string startPath);

    private sealed class WebViewScriptMessageHandler : NSObject, IWKScriptMessageHandler
    {
        private readonly Action<Uri, string> _messageReceivedAction;

        internal WebViewScriptMessageHandler(Action<Uri, string> messageReceivedAction)
        {
            _messageReceivedAction =
                messageReceivedAction ?? throw new ArgumentNullException(nameof(messageReceivedAction));
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _messageReceivedAction(AppOriginUri.Value, ((NSString)message.Body).ToString());
        }
    }

    private sealed class AppSchemeHandler : NSObject, IWKUrlSchemeHandler
    {
        private readonly BlazorWkWebView _blazorWebView;

        internal AppSchemeHandler(BlazorWkWebView blazorWebView)
        {
            _blazorWebView = blazorWebView;
        }

        [Foundation.Export("webView:startURLSchemeTask:")]
        public void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
        {
            byte[] responseBytes
                = GetResponseBytes(
                    urlSchemeTask.Request.Url.AbsoluteString,
                    out string contentType,
                    out int statusCode);

            if (statusCode != 200)
            {
                return;
            }

            using (var dic = new NSMutableDictionary<NSString, NSString>())
            {
                dic.Add(
                    (NSString)"Content-Length",
                    (NSString)responseBytes.Length.ToString(CultureInfo.InvariantCulture));
                dic.Add(
                    (NSString)"Content-Type",
                    (NSString)contentType);

                // Disable local caching. This will prevent user scripts from executing correctly.
                dic.Add(
                    (NSString)"Cache-Control",
                    (NSString)"no-cache, max-age=0, must-revalidate, no-store");

                using var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url, statusCode, "HTTP/1.1", dic);
                urlSchemeTask.DidReceiveResponse(response);
            }

            urlSchemeTask.DidReceiveData(NSData.FromArray(responseBytes));
            urlSchemeTask.DidFinish();
        }

        private byte[] GetResponseBytes(string? url, out string contentType, out int statusCode)
        {
            bool allowFallbackOnHostPage = IsUriBaseOfPage(AppOriginUri.Value, url);
            url = RemovePossibleQueryString(url);

            if (_blazorWebView._webViewManager!.TryGetResponseContentInternal(
                    url,
                    allowFallbackOnHostPage,
                    out statusCode,
                    out string statusMessage,
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

        [Foundation.Export("webView:stopURLSchemeTask:")]
        public void StopUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
        {
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
}
