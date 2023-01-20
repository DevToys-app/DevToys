#if __WASM__

using System.Reflection;
using DevToys.Api;
using DevToys.UI.Framework.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Web.WebView2.Core;
using Uno.Extensions;
using Uno.Foundation.Interop;
using Uno.Logging;
using Uno.UI.Runtime.WebAssembly;
using Windows.Foundation;

namespace DevToys.MonacoEditor;

/// <summary>
/// Provides a <div> object in the app's web page that will show the Monaco Editor.
/// </summary>
[HtmlElement("div")]
public sealed partial class CodeEditorPresenter : Control, ICodeEditorPresenter, IJSObject
{
    private static readonly string UNO_BOOTSTRAP_APP_BASE = Environment.GetEnvironmentVariable(nameof(UNO_BOOTSTRAP_APP_BASE)) ?? string.Empty;
    private static readonly string UNO_BOOTSTRAP_WEBAPP_BASE_PATH = Environment.GetEnvironmentVariable(nameof(UNO_BOOTSTRAP_WEBAPP_BASE_PATH)) ?? string.Empty;

    private readonly JSObjectHandle _handle;
    private readonly ILogger? _debugLogger;
    private readonly ILogger? _informationLogger;
    private readonly ILogger? _errorLogger;
    private readonly string _htmlId;

    public CodeEditorPresenter()
    {
        Guard.IsNotNullOrEmpty(UNO_BOOTSTRAP_APP_BASE);

        Background = new SolidColorBrush(Colors.Transparent);
        _handle = JSObjectHandle.Create(this);

        _htmlId = this.GetHtmlId();

        ILogger logger = this.Log();
        _debugLogger = logger.IsEnabled(LogLevel.Debug) ? logger : null;
        _informationLogger = logger.IsEnabled(LogLevel.Information) ? logger : null;
        _errorLogger = logger.IsEnabled(LogLevel.Error) ? logger : null;
    }

    /// <inheritdoc />
    JSObjectHandle IJSObject.Handle => _handle;

    /// <inheritdoc />
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NewWindowRequestedEventArgs>? NewWindowRequested;

    /// <inheritdoc />
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NavigationStartingEventArgs>? NavigationStarting;

    /// <inheritdoc />
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2DOMContentLoadedEventArgs>? DOMContentLoaded;

    /// <inheritdoc />
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;

    /// <inheritdoc />
    public event AsyncTypedEventHandler<ICodeEditorPresenter, EventArgs>? DotNetObjectInjectionRequested;

    /// <inheritdoc />
    public new event TypedEventHandler<ICodeEditorPresenter, RoutedEventArgs>? GotFocus;

    /// <inheritdoc />
    public new event TypedEventHandler<ICodeEditorPresenter, RoutedEventArgs>? LostFocus;

    public async Task LaunchAsync()
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

        _informationLogger?.Info($"{nameof(LaunchAsync)}: Startup");

        NavigationStarting?.Invoke(this, new CoreWebView2NavigationStartingEventArgs());
        DOMContentLoaded?.Invoke(this, new CoreWebView2DOMContentLoadedEventArgs());

        // Request to inject .NET web object into the web page.
        Guard.IsNotNull(DotNetObjectInjectionRequested);
        await DotNetObjectInjectionRequested.Invoke(this, EventArgs.Empty);

        try
        {
            _informationLogger?.Info($"{nameof(LaunchAsync)}: Creating Monaco Editor");

            string monacoEditorJavaScriptEntryPoint = $@"createMonacoEditor('{UNO_BOOTSTRAP_WEBAPP_BASE_PATH}{UNO_BOOTSTRAP_APP_BASE}/devtoys.monacoeditor', element)";
            this.ExecuteJavascript(monacoEditorJavaScriptEntryPoint);

            _informationLogger?.Info($"{nameof(LaunchAsync)}: Monaco Editor created successfully");
        }
        catch (Exception e)
        {
            _errorLogger?.Error($"{nameof(LaunchAsync)} failed", e);
        }

        NavigationCompleted?.Invoke(this, new CoreWebView2NavigationCompletedEventArgs());
    }

    public async Task InjectDotNetObjectToWebPageAsync<T>(string name, T pObject)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

        _debugLogger?.Debug($"{nameof(InjectDotNetObjectToWebPageAsync)}: Trying to inject .NET object in web page - {name}");
        if (pObject is IJSObject obj)
        {
            MethodInfo? method = obj.Handle.GetType().GetMethod("GetNativeInstance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method is null)
            {
                _errorLogger?.Error($"{nameof(InjectDotNetObjectToWebPageAsync)}: GetNativeInstance method doesn't exist.");
                return;
            }

            string? native = method.Invoke(obj.Handle, Array.Empty<object>()) as string;
            _debugLogger?.Debug($"{nameof(InjectDotNetObjectToWebPageAsync)}: Native handle {native} - {name}");

            string injectorScript
                = @$"
                    var value = {native};
                    var frame = Uno.UI.WindowManager.current.getView({_htmlId});
                    var editorContext = EditorContext.getEditorForElement(frame);
                    editorContext.{name} = value
                ";

            try
            {
                this.ExecuteJavascript(injectorScript);
                _debugLogger?.Debug($"{nameof(InjectDotNetObjectToWebPageAsync)}: '{name}' injected successfully.");
            }
            catch (Exception ex)
            {
                _errorLogger?.Error($"{nameof(InjectDotNetObjectToWebPageAsync)} failed - {name}", ex);
            }
        }
        else
        {
            _errorLogger?.Error($"{nameof(InjectDotNetObjectToWebPageAsync)}: '{name}' is not a JSObject");
            throw new InvalidOperationException($"{nameof(InjectDotNetObjectToWebPageAsync)}: '{name}' is not a JSObject");
        }
    }

    public async Task<string> InvokeScriptAsync(string script)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

        script = $@"
            (function() {{
                try {{
                    window.__evalMethod = function() {{
                        let frame = Uno.UI.WindowManager.current.getView({_htmlId});
                        let editorContext = EditorContext.getEditorForElement(frame);
                        {script}
                    }};

                    return window.eval(""__evalMethod()"") || """";
                }}
                catch(err){{
                    let frame = Uno.UI.WindowManager.current.getView({_htmlId});
                    let editorContext = EditorContext.getEditorForElement(frame);
                    editorContext.Debug.log(err);
                }}
                finally {{
                    window.__evalMethod = null;
                }}
            }})()";

        _debugLogger?.Debug($"Invoke Script: {script}");

        try
        {
            string result = this.ExecuteJavascript(script);

            _debugLogger?.Debug($"Invoke Script result: {result}");

            return result;
        }
        catch (Exception e)
        {
            _errorLogger?.Error("Invoke Script failed", e);

            return string.Empty;
        }
    }
}

#endif
