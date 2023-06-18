///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for auto indentation in the editor
/// </summary>
public enum EditorAutoIndentStrategy
{
    None = 0,

    Keep = 1,

    Brackets = 2,

    Advanced = 3,

    Full = 4
}
