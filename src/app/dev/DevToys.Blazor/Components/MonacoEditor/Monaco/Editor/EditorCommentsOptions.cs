///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// </summary>

/// <summary>
/// Configuration options for editor comments
/// </summary>
public class EditorCommentsOptions
{
    /// <summary>
    /// Insert a space after the line comment token and inside the block comments tokens.
    /// Defaults to true.
    /// </summary>
    public bool? InsertSpace { get; set; }

    /// <summary>
    /// Ignore empty lines when inserting line comments.
    /// Defaults to true.
    /// </summary>
    public bool? IgnoreEmptyLines { get; set; }
}
