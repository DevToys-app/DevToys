///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class InlineSuggestOptions
{
    /// <summary>
    /// Enable or disable the rendering of automatic inline completions.
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// Configures the mode.
    /// Use `prefix` to only show ghost text if the text to replace is a prefix of the suggestion text.
    /// Use `subword` to only show ghost text if the replace text is a subword of the suggestion text.
    /// Use `subwordSmart` to only show ghost text if the replace text is a subword of the suggestion text, but the subword must start after the cursor position.
    /// Defaults to `prefix`.
    /// </summary>
    public string? Mode { get; set; }
}
