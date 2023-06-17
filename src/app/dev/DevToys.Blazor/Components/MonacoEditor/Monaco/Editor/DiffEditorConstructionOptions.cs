///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class DiffEditorConstructionOptions : DiffEditorOptions
{
    /// <summary>
    /// The initial editor dimension (to avoid measuring the container).
    /// </summary>
    public Dimension? Dimension { get; set; }

    /// <summary>
    /// Aria label for original editor.
    /// </summary>
    public string? OriginalAriaLabel { get; set; }

    /// <summary>
    /// Aria label for modified editor.
    /// </summary>
    public string? ModifiedAriaLabel { get; set; }

    /// <summary>
    /// Is the diff editor inside another editor
    /// Defaults to false
    /// </summary>
    public bool? IsInEmbeddedEditor { get; set; }
}
