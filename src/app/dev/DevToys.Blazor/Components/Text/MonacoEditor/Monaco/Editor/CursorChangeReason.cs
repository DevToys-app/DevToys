///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Describes the reason the cursor has changed its position.
/// </summary>
public enum CursorChangeReason
{
    /// <summary>
    /// Unknown or not set.
    /// </summary>
    NotSet = 0,

    /// <summary>
    /// A `model.setValue()` was called.
    /// </summary>
    ContentFlush = 1,

    /// <summary>
    /// The `model` has been changed outside of this cursor and the cursor recovers its position from associated markers.
    /// </summary>
    RecoverFromMarkers = 2,

    /// <summary>
    /// There was an explicit user gesture.
    /// </summary>
    Explicit = 3,

    /// <summary>
    /// There was a Paste.
    /// </summary>
    Paste = 4,

    /// <summary>
    /// There was an Undo.
    /// </summary>
    Undo = 5,

    /// <summary>
    /// There was a Redo.
    /// </summary>
    Redo = 6
}
