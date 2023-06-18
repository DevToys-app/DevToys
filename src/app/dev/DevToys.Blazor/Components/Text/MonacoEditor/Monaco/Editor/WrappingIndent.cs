///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Describes how to indent wrapped lines.
/// </summary>
public enum WrappingIndent
{
    /// <summary>
    /// No indentation => wrapped lines begin at column 1.
    /// </summary>
    None = 0,

    /// <summary>
    /// Same => wrapped lines get the same indentation as the parent.
    /// </summary>
    Same = 1,

    /// <summary>
    /// Indent => wrapped lines get +1 indentation toward the parent.
    /// </summary>
    Indent = 2,

    /// <summary>
    /// DeepIndent => wrapped lines get +2 indentation toward the parent.
    /// </summary>
    DeepIndent = 3
}
