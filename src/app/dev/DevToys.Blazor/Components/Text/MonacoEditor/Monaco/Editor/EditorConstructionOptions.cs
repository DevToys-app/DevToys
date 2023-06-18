///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class EditorConstructionOptions : EditorOptions
{
    /// <summary>
    /// The initial editor dimension (to avoid measuring the container).
    /// </summary>
    public Dimension? Dimension { get; set; }
}
