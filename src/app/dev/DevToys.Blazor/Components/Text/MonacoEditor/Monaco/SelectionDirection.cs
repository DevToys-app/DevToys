///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco;

/// <summary>
/// The direction of a selection.
/// </summary>
public enum SelectionDirection
{
    /// <summary>
    /// The selection starts above where it ends.
    /// </summary>
    LTR = 0,

    /// <summary>
    /// The selection starts below where it ends.
    /// </summary>
    RTL = 1
}
