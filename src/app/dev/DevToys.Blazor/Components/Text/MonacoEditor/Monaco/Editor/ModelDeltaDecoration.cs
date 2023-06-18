///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// New model decorations.
/// </summary>
public class ModelDeltaDecoration
{
    /// <summary>
    /// Range that this decoration covers.
    /// </summary>
    public Range? Range { get; set; }

    /// <summary>
    /// Options associated with this decoration.
    /// </summary>
    public ModelDecorationOptions? Options { get; set; }
}
