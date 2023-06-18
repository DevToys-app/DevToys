///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for editor find widget
/// </summary>
public class EditorFindOptions
{
    /// <summary>
    /// Controls whether the cursor should move to find matches while typing.
    /// </summary>
    public bool? CursorMoveOnType { get; set; }

    /// <summary>
    /// Controls if we seed search string in the Find Widget with editor selection.
    /// </summary>
    public string? SeedSearchStringFromSelection { get; set; }

    /// <summary>
    /// Controls if Find in Selection flag is turned on in the editor.
    /// </summary>
    public bool? AutoFindInSelection { get; set; }

    public bool? AddExtraSpaceOnTop { get; set; }

    /// <summary>
    /// Controls whether the search automatically restarts from the beginning (or the end) when no further matches can be found
    /// </summary>
    public bool? Loop { get; set; }
}
