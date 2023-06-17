///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// A positioning preference for rendering content widgets.
/// </summary>
public enum ContentWidgetPositionPreference
{
    /// <summary>
    /// Place the content widget exactly at a position
    /// </summary>
    EXACT = 0,

    /// <summary>
    /// Place the content widget above a position
    /// </summary>
    ABOVE = 1,

    /// <summary>
    /// Place the content widget below a position
    /// </summary>
    BELOW = 2
}
