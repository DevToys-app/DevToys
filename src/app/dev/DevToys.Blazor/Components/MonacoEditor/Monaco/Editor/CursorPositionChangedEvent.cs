///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// An event describing that the cursor position has changed.
/// </summary>
public class CursorPositionChangedEvent
{
    /// <summary>
    /// Primary cursor's position.
    /// </summary>
    public Position? Position { get; set; }

    /// <summary>
    /// Secondary cursors' position.
    /// </summary>
    public List<Position>? SecondaryPositions { get; set; }

    /// <summary>
    /// Reason.
    /// </summary>
    public CursorChangeReason Reason { get; set; }

    /// <summary>
    /// Source of the call that caused the event.
    /// </summary>
    public string? Source { get; set; }
}
