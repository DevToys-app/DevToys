using DevToys.UI.Framework.Threading;
using Microsoft.Web.WebView2.Core;
using Windows.Foundation;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using DispatcherQueue = Windows.UI.Core.CoreDispatcher;
#else
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
#endif

namespace DevToys.MonacoEditor;

public interface ICodeEditorPresenter
{
    DispatcherQueue DispatcherQueue { get; }

    /// <summary>
    /// Occurs when a user performs an action in a WebView that causes content to be opened in a new window.
    /// </summary>
    event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NewWindowRequestedEventArgs>? NewWindowRequested;

    /// <summary>
    /// Occurs before the WebView navigates to new content.
    /// </summary>
    event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NavigationStartingEventArgs>? NavigationStarting;

    /// <summary>
    /// Occurs when the WebView has finished parsing the current HTML content.
    /// </summary>
    event TypedEventHandler<ICodeEditorPresenter, CoreWebView2DOMContentLoadedEventArgs>? DOMContentLoaded;

    /// <summary>
    /// Occurs when the WebView has finished loading the current content or if navigation has failed.
    /// </summary>
    event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;

    /// <summary>
    /// Occurs when the presenter needs to get allowed-web object to be injected in the web page.
    /// </summary>
    event AsyncTypedEventHandler<ICodeEditorPresenter, EventArgs>? DotNetObjectInjectionRequested;

    /// <summary>
    /// Occurs when the WebView or a component inside it (like the Monaco Editor) got focus.
    /// </summary>
    event TypedEventHandler<ICodeEditorPresenter, RoutedEventArgs>? GotFocus;

    /// <summary>
    /// Occurs when the WebView or a component inside it (like the Monaco Editor) lost focus.
    /// </summary>
    event TypedEventHandler<ICodeEditorPresenter, RoutedEventArgs>? LostFocus;

    /// <summary>
    /// Launch the web element of Monaco editor.
    /// </summary>
    Task LaunchAsync();

    /// <summary>
    /// Adds a native .NET object as a global parameter to the top level document inside of a WebView.
    /// This way, the object named by the parameter <paramref name="name"/> can be used directly in JavaScript.
    /// </summary>
    /// <param name="name">The name of the object to expose to the document in the WebView.</param>
    /// <param name="pObject">The object to expose to the document in the WebView.</param>
    Task InjectDotNetObjectToWebPageAsync<T>(string name, T pObject);

    /// <summary>
    /// Execute a JavaScript code on the web page.
    /// </summary>
    /// <returns></returns>
    Task<string> InvokeScriptAsync(string script);

    /// <summary>
    /// Give the focus to the Monaco Editor.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    bool Focus(FocusState state);
}
