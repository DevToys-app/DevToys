namespace DevToys.Linux.Components;

/// <summary>
/// Used to provide information about a link (<![CDATA[<a>]]>) clicked within a Blazor WebView.
/// <para>
/// Anchor tags with target="_blank" will always open in the default
/// browser and the UrlLoading event won't be called.
/// </para>
/// </summary>
internal sealed class UrlLoadingEventArgs : EventArgs
{
    internal static UrlLoadingEventArgs CreateWithDefaultLoadingStrategy(Uri urlToLoad, Uri appOriginUri)
    {
        UrlLoadingStrategy strategy = appOriginUri.IsBaseOf(urlToLoad) ?
            UrlLoadingStrategy.OpenInWebView :
            UrlLoadingStrategy.OpenExternally;

        return new UrlLoadingEventArgs(urlToLoad, strategy);
    }

    private UrlLoadingEventArgs(Uri url, UrlLoadingStrategy urlLoadingStrategy)
    {
        Url = url;
        UrlLoadingStrategy = urlLoadingStrategy;
    }

    /// <summary>
    /// Gets the <see cref="Url">URL</see> to be loaded.
    /// </summary>
    internal Uri Url { get; }

    /// <summary>
    /// The policy to use when loading links from the webview.
    /// Defaults to <see cref="UrlLoadingStrategy.OpenExternally"/> unless <see cref="Url"/> has a host
    /// matching the app origin, in which case the default becomes <see cref="UrlLoadingStrategy.OpenInWebView"/>.
    /// <para>
    /// This value should not be changed to <see cref="UrlLoadingStrategy.OpenInWebView"/> for external links
    /// unless you can ensure they are fully trusted.
    /// </para>
    /// </summary>
    internal UrlLoadingStrategy UrlLoadingStrategy { get; set; }
}
