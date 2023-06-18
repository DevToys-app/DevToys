///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class ContentSizeChangedEvent
{
    public double ContentWidth { get; set; }

    public double ContentHeight { get; set; }

    public bool ContentWidthChanged { get; set; }

    public bool ContentHeightChanged { get; set; }
}
