using System.Text.Json;

///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco;

public class MouseEvent
{
    public MouseEvent? BrowserEvent { get; set; }

    public bool LeftButton { get; set; }

    public bool MiddleButton { get; set; }

    public bool RightButton { get; set; }

    public int Buttons { get; set; }

    public JsonElement? Target { get; set; }

    public int Detail { get; set; }

    public double Posx { get; set; }

    public double Posy { get; set; }

    public bool CtrlKey { get; set; }

    public bool ShiftKey { get; set; }

    public bool AltKey { get; set; }

    public bool MetaKey { get; set; }

    public long Timestamp { get; set; }
}
