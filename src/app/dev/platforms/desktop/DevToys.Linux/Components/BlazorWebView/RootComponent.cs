using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;

namespace DevToys.Linux.Components;

/// <summary>
/// Describes a root component that can be added to a <see cref="BlazorWkWebView"/>.
/// </summary>
internal sealed class RootComponent
{
    /// <summary>
    /// Gets or sets the CSS selector string that specifies where in the document the component should be placed.
    /// This must be unique among the root components within the <see cref="BlazorWkWebView"/>.
    /// </summary>
    internal string? Selector { get; init; }

    /// <summary>
    /// Gets or sets the type of the root component. This type must implement <see cref="System.ComponentModel.IComponent"/>.
    /// </summary>
    internal Type? ComponentType { get; init; }

    /// <summary>
    /// Gets or sets an optional dictionary of parameters to pass to the root component.
    /// </summary>
    internal IDictionary<string, object?>? Parameters { get; set; }

    internal Task AddToWebViewManagerAsync(WebViewManager webViewManager)
    {
        // As a characteristic of XAML,we can't rely on non-default constructors. So we have to
        // validate that the required properties were set. We could skip validating this and allow
        // the lower-level renderer code to throw, but that would be harder for developers to understand.

        if (string.IsNullOrWhiteSpace(Selector))
        {
            throw new InvalidOperationException($"{nameof(RootComponent)} requires a value for its {nameof(Selector)} property, but no value was set.");
        }

        if (ComponentType is null)
        {
            throw new InvalidOperationException($"{nameof(RootComponent)} requires a value for its {nameof(ComponentType)} property, but no value was set.");
        }

        ParameterView parameterView = Parameters == null ? ParameterView.Empty : ParameterView.FromDictionary(Parameters);
        return webViewManager.AddRootComponentAsync(ComponentType, Selector, parameterView);
    }

    internal Task RemoveFromWebViewManagerAsync(WebViewManager webviewManager)
    {
        if (string.IsNullOrWhiteSpace(Selector))
        {
            throw new InvalidOperationException( $"{nameof(RootComponent)} requires a value for its {nameof(Selector)} property, but no value was set.");
        }

        return webviewManager.RemoveRootComponentAsync(Selector);
    }
}
