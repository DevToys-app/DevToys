///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class TextModelResolvedOptions
{
    public int TabSize { get; set; }

    public int IndentSize { get; set; }

    public bool InsertSpaces { get; set; }

    public DefaultEndOfLine DefaultEOL { get; set; }

    public bool TrimAutoWhitespace { get; set; }

    public BracketPairColorizationOptions? BracketPairColorizationOptions { get; set; }
}
