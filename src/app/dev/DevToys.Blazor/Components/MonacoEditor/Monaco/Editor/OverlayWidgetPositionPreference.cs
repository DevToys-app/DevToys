///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// A positioning preference for rendering overlay widgets.
/// </summary>
public enum OverlayWidgetPositionPreference
{
    /// <summary>
    /// Position the overlay widget in the top right corner
    /// </summary>
    TOP_RIGHT_CORNER = 0,

    /// <summary>
    /// Position the overlay widget in the bottom right corner
    /// </summary>
    BOTTOM_RIGHT_CORNER = 1,

    /// <summary>
    /// Position the overlay widget in the top center
    /// </summary>
    TOP_CENTER = 2
}
