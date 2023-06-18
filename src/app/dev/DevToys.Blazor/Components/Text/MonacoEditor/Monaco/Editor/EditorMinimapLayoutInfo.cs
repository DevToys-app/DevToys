///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// The public layout details of the editor.
/// </summary>
public class EditorMinimapLayoutInfo
{
    public RenderMinimap RenderMinimap { get; set; }

    public float MinimapLeft { get; set; }

    public float MinimapWidth { get; set; }

    public bool MinimapHeightIsEditorHeight { get; set; }

    public bool MinimapIsSampling { get; set; }

    public float MinimapScale { get; set; }

    public float MinimapLineHeight { get; set; }

    public float MinimapCanvasInnerWidth { get; set; }

    public float MinimapCanvasInnerHeight { get; set; }

    public float MinimapCanvasOuterWidth { get; set; }

    public float MinimapCanvasOuterHeight { get; set; }
}
