///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for parameter hints
/// </summary>
public class EditorParameterHintOptions
{
    /// <summary>
    /// Enable parameter hints.
    /// Defaults to true.
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// Enable cycling of parameter hints.
    /// Defaults to false.
    /// </summary>
    public bool? Cycle { get; set; }
}
