#if __MAC__

using Windows.Foundation;
using DevToys.UI.Framework.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using DispatcherQueue = Windows.UI.Core.CoreDispatcher;

namespace DevToys.MonacoEditor;

/// <summary>
/// Provides a WebView that displays the Monaco Editor.
/// </summary>
public sealed partial class CodeEditorPresenter : UserControl, ICodeEditorPresenter
{
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NewWindowRequestedEventArgs>? NewWindowRequested;
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NavigationStartingEventArgs>? NavigationStarting;
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2DOMContentLoadedEventArgs>? DOMContentLoaded;
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;
    public event AsyncTypedEventHandler<ICodeEditorPresenter, EventArgs>? DotNetObjectInjectionRequested;
    public event TypedEventHandler<ICodeEditorPresenter, RoutedEventArgs>? GotFocus;
    public event TypedEventHandler<ICodeEditorPresenter, RoutedEventArgs>? LostFocus;
    
    public Task LaunchAsync()
    {
        return Task.CompletedTask;
    }

    public Task InjectDotNetObjectToWebPageAsync<T>(string name, T pObject)
    {
        return Task.CompletedTask;
    }

    public Task<string> InvokeScriptAsync(string script)
    {
        return Task.FromResult(string.Empty);
    }
}

#endif
