///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class ModelOptionsChangedEvent
{
    public bool TabSize { get; set; }

    public bool IndentSize { get; set; }

    public bool InsertSpaces { get; set; }

    public bool TrimAutoWhitespace { get; set; }
}
