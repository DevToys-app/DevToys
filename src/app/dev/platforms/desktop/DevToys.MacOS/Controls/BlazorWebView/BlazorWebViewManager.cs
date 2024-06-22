using System.Text.Encodings.Web;
using DevToys.Api;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebKit;

namespace DevToys.MacOS.Controls.BlazorWebView;

/// <summary>
/// An implementation of <see cref="WebViewManager"/> that uses the <see cref="WKWebView"/> browser control
/// to render web content.
/// </summary>
public partial class BlazorWebViewManager : WebViewManager
{
    private readonly BlazorWkWebView _blazorMauiWebViewHandler;
    private readonly ILogger _logger;
    private readonly WKWebView _webview;
    private readonly string _contentRootRelativeToAppRoot;

    /// <summary>
    /// Initializes a new instance of <see cref="WebViewManager"/>
    /// </summary>
    /// <param name="blazorMauiWebViewHandler">The <see cref="BlazorWkWebView"/>.</param>
    /// <param name="provider">The <see cref="IServiceProvider"/> for the application.</param>
    /// <param name="fileProvider">Provides static content to the webview.</param>
    /// <param name="jsComponents">Describes configuration for adding, removing, and updating root components from JavaScript code.</param>
    /// <param name="contentRootRelativeToAppRoot">Path to the directory containing application content files.</param>
    /// <param name="hostPageRelativePath">Path to the host page within the fileProvider.</param>
    internal BlazorWebViewManager(
        Uri baseUri,
        BlazorWkWebView blazorMauiWebViewHandler,
        IServiceProvider provider,
        IFileProvider fileProvider,
        JSComponentConfigurationStore jsComponents,
        string contentRootRelativeToAppRoot,
        string hostPageRelativePath)
        : base(
            provider,
            new AppKitDispatcher(),
            baseUri,
            fileProvider,
            jsComponents,
            hostPageRelativePath)
    {
        Guard.IsNotNull(blazorMauiWebViewHandler);
        Guard.IsNotNull(blazorMauiWebViewHandler.View);

        _logger = this.Log();
        _blazorMauiWebViewHandler = blazorMauiWebViewHandler;
        _webview = blazorMauiWebViewHandler.View;
        _contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;

        InitializeWebView();
    }

    /// <inheritdoc />
    protected override void NavigateCore(Uri absoluteUri)
    {
        LogNavigatingToUri(absoluteUri);
        using var nsUrl = new NSUrl(absoluteUri.ToString());
        using var request = new NSUrlRequest(nsUrl);
        _webview.LoadRequest(request);
    }

    internal bool TryGetResponseContentInternal(
        string uri,
        bool allowFallbackOnHostPage,
        out int statusCode,
        out string statusMessage,
        out Stream content,
        out IDictionary<string, string> headers)
    {
        bool defaultResult
            = TryGetResponseContent(
                uri,
                allowFallbackOnHostPage,
                out statusCode,
                out statusMessage,
                out content,
                out headers);
        bool hotReloadedResult
            = StaticContentHotReloadManager.TryReplaceResponseContent(
                _contentRootRelativeToAppRoot,
                uri,
                ref statusCode,
                ref content,
                headers);
        return defaultResult || hotReloadedResult;
    }

    /// <inheritdoc />
    protected override void SendMessage(string message)
    {
        string messageJsStringLiteral = JavaScriptEncoder.Default.Encode(message);
        _webview.EvaluateJavaScript(
            javascript: $"__dispatchMessageCallback(\"{messageJsStringLiteral}\")",
            completionHandler: (NSObject result, NSError error) => { });
    }

    internal void MessageReceivedInternal(Uri uri, string message)
    {
        MessageReceived(uri, message);
    }

    private void InitializeWebView()
    {
        _webview.NavigationDelegate = new WebViewNavigationDelegate(_blazorMauiWebViewHandler);
        _webview.UIDelegate = new WebViewUiDelegate();
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Navigating to {uri}.")]
    partial void LogNavigatingToUri(Uri uri);

    private sealed class WebViewUiDelegate : WKUIDelegate
    {
        private static readonly string localOk = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("OK");

        private static readonly string localCancel = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Cancel");

        public override void RunJavaScriptAlertPanel(
            WKWebView webView,
            string message,
            WKFrameInfo frame,
            Action completionHandler)
        {
            var alert = new NSAlert { AlertStyle = NSAlertStyle.Informational, InformativeText = message };
            alert.RunModal();
            completionHandler();
        }

        public override void RunJavaScriptConfirmPanel(
            WKWebView webView,
            string message,
            WKFrameInfo frame,
            Action<bool> completionHandler)
        {
            var alert = new NSAlert { AlertStyle = NSAlertStyle.Informational, InformativeText = message, };
            alert.AddButton(localOk);
            alert.AddButton(localCancel);
            alert.BeginSheetForResponse(webView.Window, (result) =>
            {
                bool okButtonClicked = result == 1000;
                completionHandler(okButtonClicked);
            });
        }

        public override void RunJavaScriptTextInputPanel(
            WKWebView webView, string prompt, string? defaultText, WKFrameInfo frame, Action<string> completionHandler)
        {
            var alert = new NSAlert { AlertStyle = NSAlertStyle.Informational, InformativeText = prompt, };
            var textField = new NSTextField(new CGRect(0, 0, 300, 20)) { PlaceholderString = defaultText, };
            alert.AccessoryView = textField;
            alert.AddButton(localOk);
            alert.AddButton(localCancel);
            alert.BeginSheetForResponse(webView.Window, (result) =>
            {
                bool okButtonClicked = result == 1000;
                completionHandler(okButtonClicked ? textField.StringValue : null);
            });
        }
    }

    private sealed class WebViewNavigationDelegate : WKNavigationDelegate
    {
        private readonly BlazorWkWebView _webView;

        private WKNavigation? _currentNavigation;
        private Uri? _currentUri;

        public WebViewNavigationDelegate(BlazorWkWebView webView)
        {
            Guard.IsNotNull(webView);
            _webView = webView;
        }

        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            _currentNavigation = navigation;
        }

        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            NSUrl requestUrl = navigationAction.Request.Url;
            var uri = new Uri(requestUrl.ToString());

            UrlLoadingStrategy strategy;

            // TargetFrame is null for navigation to a new window (`_blank`)
            if (navigationAction.TargetFrame is null)
            {
                // Open in a new browser window regardless of UrlLoadingStrategy
                strategy = UrlLoadingStrategy.OpenExternally;
            }
            else
            {
                // Invoke the UrlLoading event to allow overriding the default link handling behavior
                var callbackArgs = UrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, BlazorWkWebView.AppOriginUri);
                _webView.OnUrlLoading(callbackArgs);

                strategy = callbackArgs.UrlLoadingStrategy;
            }

            if (strategy == UrlLoadingStrategy.OpenExternally)
            {
                NSWorkspace.SharedWorkspace.OpenUrl(requestUrl);
            }

            if (strategy != UrlLoadingStrategy.OpenInWebView)
            {
                // Cancel any further navigation as we've either opened the link in the external browser
                // or canceled the underlying navigation action.
                decisionHandler(WKNavigationActionPolicy.Cancel);
                return;
            }

            if (navigationAction.TargetFrame!.MainFrame)
            {
                _currentUri = requestUrl;
            }

            decisionHandler(WKNavigationActionPolicy.Allow);
        }

        public override void DidReceiveServerRedirectForProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            // We need to intercept the redirects to the app scheme because Safari will block them.
            // We will handle these redirects through the Navigation Manager.
            if (_currentUri?.Host != BlazorWkWebView.AppHostAddress)
            {
                return;
            }

            Uri? uri = _currentUri;
            _currentUri = null;
            _currentNavigation = null;

            if (uri is null)
            {
                return;
            }

            var request = new NSUrlRequest(new NSUrl(uri.AbsoluteUri));
            webView.LoadRequest(request);
        }

        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            _currentUri = null;
            _currentNavigation = null;
        }

        public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            _currentUri = null;
            _currentNavigation = null;
        }

        public override void DidCommitNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (_currentUri != null && _currentNavigation == navigation)
            {
                // TODO: Determine whether this is needed
                //_webView.HandleNavigationStarting(_currentUri);
            }
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (_currentUri != null && _currentNavigation == navigation)
            {
                // TODO: Determine whether this is needed
                //_webView.HandleNavigationFinished(_currentUri);
                _currentUri = null;
                _currentNavigation = null;
            }
        }
    }
}
