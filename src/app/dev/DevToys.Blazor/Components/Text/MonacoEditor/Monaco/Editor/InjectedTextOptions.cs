///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configures text that is injected into the view without changing the underlying document.
/// </summary>
public class InjectedTextOptions
{
    /// <summary>
    /// Sets the text to inject. Must be a single line.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// If set, the decoration will be rendered inline with the text with this CSS class name.
    /// </summary>
    public string? InlineClassName { get; set; }

    /// <summary>
    /// If there is an `inlineClassName` which affects letter spacing.
    /// </summary>
    public bool? InlineClassNameAffectsLetterSpacing { get; set; }

    /// <summary>
    /// Configures cursor stops around injected text.
    /// Defaults to {@link InjectedTextCursorStops.Both}.
    /// </summary>
    public InjectedTextCursorStops? CursorStops { get; set; }
}
