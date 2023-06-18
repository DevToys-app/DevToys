///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// The default end of line to use when instantiating models.
/// </summary>
public enum DefaultEndOfLine
{
    /// <summary>
    /// Use line feed (\n) as the end of line character.
    /// </summary>
    LF = 1,

    /// <summary>
    /// Use carriage return and line feed (\r\n) as the end of line character.
    /// </summary>
    CRLF = 2
}
