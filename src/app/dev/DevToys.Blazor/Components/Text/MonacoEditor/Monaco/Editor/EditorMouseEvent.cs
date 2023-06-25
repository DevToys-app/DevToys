///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// A mouse event originating from the editor.
/// </summary>
public class EditorMouseEvent
{
    public MouseEvent? Event { get; set; }

    public BaseMouseTarget? Target { get; set; }
}
