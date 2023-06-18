using System.Text.Json;

///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco;

public class KeyboardEvent
{
    public KeyboardEvent? BrowserEvent { get; set; }

    public JsonElement? Target { get; set; }

    public bool CtrlKey { get; set; }

    public bool ShiftKey { get; set; }

    public bool AltKey { get; set; }

    public bool MetaKey { get; set; }

    public KeyCode KeyCode { get; set; }

    public string? Code { get; set; }
}
