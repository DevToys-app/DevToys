using OneOf;

namespace DevToys.Api;

/// <summary>
/// A component that represents a web view.
/// </summary>
public interface IUIWebView : IUITitledElement
{
    /// <summary>
    /// The HTML content to display, or URI to the page to display.
    /// </summary>
    OneOf<string, Uri>? Source { get; }

    /// <summary>
    /// Raised when <see cref="Source"/> is changed.
    /// </summary>
    event EventHandler? SourceChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Source = {{{nameof(Source)}}}")]
internal sealed class UIWebView : UITitledElement, IUIWebView
{
    private OneOf<string, Uri>? _source = default;

    internal UIWebView(string? id)
        : base(id)
    {
    }

    public OneOf<string, Uri>? Source
    {
        get => _source;
        internal set => SetPropertyValue(ref _source, value, SourceChanged);
    }

    public event EventHandler? SourceChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that displays a web page.
    /// </summary>
    public static IUIWebView WebView()
    {
        return WebView(id: null);
    }

    /// <summary>
    /// A component that displays a web page.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIWebView WebView(string? id)
    {
        return new UIWebView(id);
    }

    /// <summary>
    /// Sets the <see cref="IUIWebView.Source"/> from an HTML document.
    /// </summary>
    public static IUIWebView RenderHTML(this IUIWebView element, string html)
    {
        ((UIWebView)element).Source = html;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIWebView.Source"/> from a <see cref="Uri"/>.
    /// </summary>
    public static IUIWebView NavigateToUri(this IUIWebView element, Uri uri)
    {
        ((UIWebView)element).Source = uri;
        return element;
    }
}
