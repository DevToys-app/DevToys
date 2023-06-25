///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco;

/// <summary>
/// A position in the editor. This interface is suitable for serialization.
/// </summary>
public class Position
{
    /// <summary>
    /// line number (starts at 1)
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// column (the first character in a line is between column 1 and column 2)
    /// </summary>
    public int Column { get; set; }
}
