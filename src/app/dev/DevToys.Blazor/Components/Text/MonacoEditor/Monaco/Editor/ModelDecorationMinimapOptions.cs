///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Options for rendering a model decoration in the overview ruler.
/// </summary>
public class ModelDecorationMinimapOptions : DecorationOptions
{
    /// <summary>
    /// The position in the overview ruler.
    /// </summary>
    public MinimapPosition Position { get; set; }
}
