///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for editor padding
/// </summary>
public class EditorPaddingOptions
{
    /// <summary>
    /// Spacing between top edge of editor and first line.
    /// </summary>
    public float? Top { get; set; }

    /// <summary>
    /// Spacing between bottom edge of editor and last line.
    /// </summary>
    public float? Bottom { get; set; }
}
