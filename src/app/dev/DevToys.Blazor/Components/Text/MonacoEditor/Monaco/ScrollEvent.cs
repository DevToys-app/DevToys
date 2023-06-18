///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco;

public class ScrollEvent
{
    public double ScrollTop { get; set; }

    public double ScrollLeft { get; set; }

    public double ScrollWidth { get; set; }

    public double ScrollHeight { get; set; }

    public bool ScrollTopChanged { get; set; }

    public bool ScrollLeftChanged { get; set; }

    public bool ScrollWidthChanged { get; set; }

    public bool ScrollHeightChanged { get; set; }
}
